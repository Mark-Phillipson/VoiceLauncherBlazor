using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

/// <summary>
/// Applies AI review suggestions (from AIReviewResult) back to persisted files such as
/// CursorlessManualQuestions.json. Currently supports updating manual question prompts.
/// </summary>
public class AIReviewApplier
{
    private readonly IWebHostEnvironment _env;
    private readonly IDbContextFactory<ApplicationDbContext>? _dbFactory;
    private readonly ILogger<AIReviewApplier> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public AIReviewApplier(IWebHostEnvironment env, IDbContextFactory<ApplicationDbContext>? dbFactory, ILogger<AIReviewApplier> logger)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _dbFactory = dbFactory;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Applies suggested prompts to the persisted CursorlessManualQuestions.json file.
    /// Matching is performed by comparing the review result's OriginalPrompt to the
    /// persisted question's Prompt. Returns the number of updated entries.
    /// </summary>
    public async Task<int> ApplyResultsToManualFileAsync(IEnumerable<AIReviewResult> results, CancellationToken ct = default)
    {
        if (results == null) return 0;

        var candidates = results.Where(r => !string.IsNullOrWhiteSpace(r.SuggestedPrompt) && !string.Equals(r.SuggestedPrompt, r.OriginalPrompt, StringComparison.Ordinal)).ToList();
        if (candidates.Count == 0) return 0;

        var webRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath) ? _env.WebRootPath : Path.Combine(_env.ContentRootPath, "wwwroot");
        var filePath = Path.Combine(webRoot, "CursorlessManualQuestions.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Manual questions file not found at {path}", filePath);
            return 0;
        }

        var json = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(json, JsonOptions) ?? new List<QuizQuestion>();

        var updated = 0;
        foreach (var r in candidates)
        {
            if (string.IsNullOrWhiteSpace(r.OriginalPrompt)) continue;
            var match = questions.FirstOrDefault(q => string.Equals(q.Prompt?.Trim(), r.OriginalPrompt?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                match.Prompt = r.SuggestedPrompt!;
                // mark that this question was corrected by the AI so UI can indicate it
                match.CorrectedByAI = true;
                updated++;
            }
        }

        if (updated > 0)
        {
            var outJson = JsonSerializer.Serialize(questions, JsonOptions);
            await File.WriteAllTextAsync(filePath, outJson, ct).ConfigureAwait(false);
            _logger.LogInformation("Applied {count} suggested prompts to {file}", updated, filePath);
        }

        return updated;
    }
}
