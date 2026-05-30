using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public class OpenAIReviewService : IAIReviewService
{
    private readonly IConfiguration _cfg;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<OpenAIReviewService> _logger;
    private readonly Channel<QuizQuestion[]> _channel = Channel.CreateUnbounded<QuizQuestion[]>();
    private readonly bool _enabled;
    private readonly string? _apiKeyEnvVar;
    private readonly string? _apiKey;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly int _chunkSize;

    public OpenAIReviewService(IConfiguration cfg, IHttpClientFactory httpFactory, ILogger<OpenAIReviewService> logger)
    {
        _cfg = cfg;
        _httpFactory = httpFactory;
        _logger = logger;

        _enabled = bool.TryParse(_cfg["AIReview:Enabled"], out var en) ? en : true;
        _apiKeyEnvVar = _cfg["AIReview:OpenAI:ApiKeyEnvVar"] ?? "OPENAI_API_KEY";
        _apiKey = Environment.GetEnvironmentVariable(_apiKeyEnvVar ?? "OPENAI_API_KEY");
        _endpoint = _cfg["AIReview:OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        _model = _cfg["AIReview:OpenAI:Model"] ?? "gpt-4o-mini";
        _chunkSize = int.TryParse(_cfg["AIReview:ChunkSize"], out var c) ? Math.Max(1, c) : 10;
    }

    public ChannelReader<QuizQuestion[]> GetReviewReader() => _channel.Reader;

    public async Task EnqueueForReviewAsync(IEnumerable<QuizQuestion> questions, CancellationToken ct = default)
    {
        if (!_enabled)
            return;

        if (questions == null) return;
        var arr = questions.ToArray();
        for (int i = 0; i < arr.Length; i += _chunkSize)
        {
            var chunk = arr.Skip(i).Take(_chunkSize).ToArray();
            await _channel.Writer.WriteAsync(chunk, ct).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<Models.AIReviewResult>> ReviewQuestionsAsync(IEnumerable<QuizQuestion> questions, CancellationToken ct = default)
    {
        if (!_enabled)
            return Array.Empty<Models.AIReviewResult>();

        var list = questions?.ToArray() ?? Array.Empty<QuizQuestion>();
        if (list.Length == 0) return Array.Empty<Models.AIReviewResult>();

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("OpenAI API key not configured (env var '{envVar}'). Skipping review. Set the key to enable 'Only AI-corrected' quiz filtering.", _apiKeyEnvVar);
            return Array.Empty<Models.AIReviewResult>();
        }

        try
        {
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            // Prepare minimal JSON payload describing each question
            var payloadQuestions = list.Select((q, idx) => new
            {
                index = idx,
                prompt = q.Prompt,
                choices = q.Choices,
                category = q.Category,
                source = q.Source
            }).ToArray();

            var systemInstruction = "You are a helpful assistant that rewrites short quiz prompts to be clear, concise, and use correct terminology.\n\nInput: a JSON array of objects with fields: index, prompt, choices, category, source.\nOutput: a JSON array of objects matching input order, each with: { index, ok (boolean), suggestedPrompt (string|null), notes (string) }. Return ONLY valid JSON (the array).";

            var userContent = JsonSerializer.Serialize(payloadQuestions);

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemInstruction },
                    new { role = "user", content = userContent }
                },
                temperature = 0.2,
                max_tokens = 800
            };

            var bodyJson = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            using var res = await client.PostAsync(_endpoint, content, ct).ConfigureAwait(false);
            var resText = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI review request failed: {status} - {text}", res.StatusCode, resText);
                return Array.Empty<Models.AIReviewResult>();
            }

            // Try to extract the assistant message content and parse JSON array inside it
            using var doc = JsonDocument.Parse(resText);
            if (!doc.RootElement.TryGetProperty("choices", out var choicesElem) || choicesElem.GetArrayLength() == 0)
            {
                _logger.LogWarning("OpenAI response missing choices. Raw: {raw}", resText);
                return Array.Empty<Models.AIReviewResult>();
            }

            var messageContent = string.Empty;
            try
            {
                messageContent = choicesElem[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }
            catch
            {
                // fallback: try to find any string content in response
                messageContent = doc.RootElement.ToString();
            }

            // Extract first JSON array in the message content
            var start = messageContent.IndexOf('[');
            var end = messageContent.LastIndexOf(']');
            if (start < 0 || end <= start)
            {
                _logger.LogWarning("OpenAI reply did not contain JSON array. Reply: {reply}", messageContent);
                return Array.Empty<Models.AIReviewResult>();
            }

            var jsonArray = messageContent.Substring(start, end - start + 1);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var results = JsonSerializer.Deserialize<List<Models.AIReviewResult>>(jsonArray, opts) ?? new List<Models.AIReviewResult>();

            // Attach original prompts for traceability
            foreach (var r in results)
            {
                if (r.Index >= 0 && r.Index < list.Length)
                    r.OriginalPrompt = list[r.Index].Prompt;
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling OpenAI for review");
            return Array.Empty<Models.AIReviewResult>();
        }
    }
}
