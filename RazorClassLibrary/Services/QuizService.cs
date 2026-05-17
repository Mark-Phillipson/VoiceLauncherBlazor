using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using DataAccessLibrary.DTOs;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Hosting;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public sealed class QuizService : IQuizService
{
    private const string DocsFileName = "CursorlessDocsQuizData.json";
    private const string ManualFileName = "CursorlessManualQuestions.json";
    private const string DefaultPackId = "cursorless"; // For backward compatibility
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ICursorlessCheatsheetItemDataService _cheatsheetDataService;
    private readonly IWebHostEnvironment _environment;
    private readonly IQuizContentLoader? _contentLoader;
    private readonly ILogger<QuizService> _logger;

    // Legacy constructor for backward compatibility
    public QuizService(ICursorlessCheatsheetItemDataService cheatsheetDataService, IWebHostEnvironment environment)
    {
        _cheatsheetDataService = cheatsheetDataService;
        _environment = environment;
        _contentLoader = null;
        _logger = null!;
    }

    // New constructor with multi-pack support
    public QuizService(
        ICursorlessCheatsheetItemDataService cheatsheetDataService,
        IWebHostEnvironment environment,
        IQuizContentLoader contentLoader,
        ILogger<QuizService> logger)
    {
        _cheatsheetDataService = cheatsheetDataService;
        _environment = environment;
        _contentLoader = contentLoader;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count)
    {
        // Legacy method: defaults to Cursorless pack for backward compatibility
        return await GenerateQuestionsAsync(count, DefaultPackId, categoryFilter: null);
    }

    /// <summary>
    /// Generates quiz questions from a specific pack, optionally filtered by category.
    /// </summary>
    /// <param name="count">Number of questions to generate.</param>
    /// <param name="packId">Quiz pack ID (e.g., "cursorless", "talon").</param>
    /// <param name="categoryFilter">Optional category ID to filter facts. Null = all categories.</param>
    /// <returns>List of generated or loaded quiz questions.</returns>
    public async Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count, string packId, string? categoryFilter = null)
    {
        if (count <= 0)
        {
            return Array.Empty<QuizQuestion>();
        }

        // Use new content loader if available (multi-pack mode)
        if (_contentLoader != null)
        {
            var facts = await _contentLoader.LoadFactsAsync(packId);
            var manualQuestions = await _contentLoader.LoadManualQuestionsAsync(packId);

            // Filter by category if specified
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                facts = facts
                    .Where(f => string.Equals(f.Category, categoryFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                manualQuestions = manualQuestions
                    .Where(q => string.Equals(q.Category, categoryFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return await GenerateQuestionsFromFactsAsync(count, facts, manualQuestions);
        }

        // Legacy fallback: use existing Cursorless-specific logic
        var legacyFacts = await LoadFactsAsync();
        var legacyManualQuestions = await LoadManualQuestionsAsync();
        return await GenerateQuestionsFromFactsAsync(count, legacyFacts, legacyManualQuestions);
    }

    /// <summary>
    /// Internal method to generate questions from a set of facts and manual questions.
    /// </summary>
    private async Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsFromFactsAsync(
        int count,
        IReadOnlyList<QuizFact> facts,
        IReadOnlyList<QuizQuestion> manualQuestions)
    {
        await Task.CompletedTask; // Ensure this method can be awaited
        
        if ((facts.Count + manualQuestions.Count) == 0)
        {
            return Array.Empty<QuizQuestion>();
        }

        // Mix manual questions with generated facts so quizzes cover more topics
        // (avoid manual-only quizzes when manualQuestions count is large).
        var manualToTake = Math.Min(manualQuestions.Count, Math.Max(0, count / 2));
        var selectedManual = manualQuestions
            .OrderBy(_ => Random.Shared.Next())
            .Take(manualToTake)
            .ToList();

        var generateCount = Math.Max(0, count - selectedManual.Count);
        var selectedFacts = facts
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(generateCount, facts.Count))
            .ToList();

        var generatedQuestions = selectedFacts
            .Select(fact => BuildQuestion(fact, facts))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var combined = selectedManual
            .Concat(generatedQuestions)
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();

        // If still short (not enough facts), fill remaining slots from manual questions.
        if (combined.Count < count && manualQuestions.Count > selectedManual.Count)
        {
            var extras = manualQuestions
                .Where(q => !combined.Contains(q))
                .OrderBy(_ => Random.Shared.Next())
                .Take(count - combined.Count);
            combined = combined.Concat(extras).ToList();
        }

        return combined;
    }

    public List<QuizScore> DeserializeScores(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<QuizScore>();
        }

        return JsonSerializer.Deserialize<List<QuizScore>>(json, JsonOptions) ?? new List<QuizScore>();
    }

    public string SerializeScores(IEnumerable<QuizScore> scores)
    {
        return JsonSerializer.Serialize(scores, JsonOptions);
    }

    private async Task<List<QuizFact>> LoadFactsAsync()
    {
        var facts = new List<QuizFact>();

        var cheatsheetItems = await _cheatsheetDataService.GetAllCursorlessCheatsheetItemsAsync(getFromJson: true);
        facts.AddRange(cheatsheetItems.Select(item => new QuizFact(
            Id: item.Id.ToString(),
            SpokenForm: item.SpokenForm.Trim(),
            Meaning: item.Meaning.Trim(),
            Category: string.IsNullOrWhiteSpace(item.CursorlessType) ? "Cursorless" : item.CursorlessType.Trim(),
            YoutubeLink: item.YoutubeLink,
            Source: "cheatsheet")));
        
        facts.AddRange(await LoadDocsFactsAsync());

        return facts
            .Where(fact => !string.IsNullOrWhiteSpace(fact.SpokenForm) && !string.IsNullOrWhiteSpace(fact.Meaning))
            .DistinctBy(fact => new { fact.SpokenForm, fact.Meaning, fact.Category })
            .ToList();
    }

    private async Task<List<QuizFact>> LoadDocsFactsAsync()
    {
        var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(_environment.ContentRootPath, "wwwroot");

        var filePath = Path.Combine(webRootPath, DocsFileName);
        if (!File.Exists(filePath))
        {
            return new List<QuizFact>();
        }

        var json = await File.ReadAllTextAsync(filePath);
        var items = JsonSerializer.Deserialize<List<CursorlessCheatsheetItemDTO>>(json, JsonOptions) ?? new List<CursorlessCheatsheetItemDTO>();
        return items.Select(item => new QuizFact(
            Id: item.Id.ToString(),
            SpokenForm: item.SpokenForm.Trim(),
            Meaning: item.Meaning.Trim(),
            Category: string.IsNullOrWhiteSpace(item.CursorlessType) ? "Cursorless" : item.CursorlessType.Trim(),
            YoutubeLink: item.YoutubeLink,
            Source: "docs")).ToList();
    }

    private async Task<List<QuizQuestion>> LoadManualQuestionsAsync()
    {
        var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(_environment.ContentRootPath, "wwwroot");

        var filePath = Path.Combine(webRootPath, ManualFileName);
        if (!File.Exists(filePath))
        {
            return new List<QuizQuestion>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var items = JsonSerializer.Deserialize<List<QuizQuestion>>(json, JsonOptions) ?? new List<QuizQuestion>();

            // Normalize question-level image paths, but DO NOT auto-attach per-choice thumbnails.
            // Auto-attaching per-choice images revealed correct answers; only use an explicit
            // question-level `Image` when provided.
            foreach (var q in items)
            {
                if (!string.IsNullOrWhiteSpace(q.Image))
                {
                    q.Image = q.Image.Replace('\\', '/');
                }

                // Ensure the ChoiceImagePaths list aligns with choices (null = no image).
                q.ChoiceImagePaths = q.Choices?.Select(_ => (string?)null).ToList() ?? new List<string?>();

                // Shuffle manual questions (especially hat-shape entries) so the correct
                // answer isn't always in the first position. This avoids positional bias
                // when the JSON author put the correct answer first.
                if (string.Equals(q.Source, "manual", StringComparison.OrdinalIgnoreCase)
                    && q.Choices != null && q.Choices.Count > 1)
                {
                    var paired = q.Choices
                        .Select((c, i) => new { Choice = c, Image = (q.ChoiceImagePaths != null && q.ChoiceImagePaths.Count > i) ? q.ChoiceImagePaths[i] : null })
                        .OrderBy(_ => Random.Shared.Next())
                        .ToList();

                    q.Choices = paired.Select(p => p.Choice).ToList();
                    q.ChoiceImagePaths = paired.Select(p => p.Image).ToList();

                    q.CorrectIndex = q.Choices.FindIndex(c => string.Equals(c, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
                    if (q.CorrectIndex < 0)
                    {
                        // Sanity: if correct answer somehow missing, ensure it's present at 0
                        q.Choices[0] = q.CorrectAnswer;
                        q.CorrectIndex = 0;
                    }
                }

                // Sanitize manual prompts: redact explicit occurrences of the correct answer
                // to avoid leaking answers inside the question text.
                if (string.Equals(q.Source, "manual", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(q.CorrectAnswer)
                    && !string.IsNullOrWhiteSpace(q.Prompt)
                    && WouldLeakAnswer(q.Prompt, q.CorrectAnswer))
                {
                    var redacted = RedactAnswerTokens(q.Prompt, q.CorrectAnswer);
                    if (!string.IsNullOrWhiteSpace(redacted))
                    {
                        q.Prompt = redacted;
                    }
                    else
                    {
                        Debug.WriteLine($"[QuizService] Manual prompt redaction produced empty prompt for answer '{q.CorrectAnswer}'");
                    }
                }
            }

            return items;
        }
        catch
        {
            return new List<QuizQuestion>();
        }
    }

    private static QuizQuestion BuildQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        return Random.Shared.Next(3) switch
        {
            0 => BuildSpokenToMeaningQuestion(fact, facts),
            1 => BuildMeaningToSpokenQuestion(fact, facts),
            _ => BuildCategoryQuestion(fact, facts)
        };
    }

    private static QuizQuestion BuildSpokenToMeaningQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        var correctAnswer = fact.Meaning;
        var distractors = GatherDistractors(fact, facts, candidate => candidate.Meaning, correctAnswer);
        var prompt = $"What does '{fact.SpokenForm}' do?";
        if (WouldLeakAnswer(prompt, correctAnswer))
        {
            var redacted = RedactAnswerTokens(prompt, correctAnswer);
            if (!WouldLeakAnswer(redacted, correctAnswer) && !string.IsNullOrWhiteSpace(redacted))
            {
                prompt = redacted;
            }
            else
            {
                return BuildCategoryQuestion(fact, facts);
            }
        }

        return CreateQuestion(prompt, fact.Category, fact.Source ?? "generated", correctAnswer, distractors);
    }

    private static QuizQuestion BuildMeaningToSpokenQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        var correctAnswer = fact.SpokenForm;
        var distractors = GatherDistractors(fact, facts, candidate => candidate.SpokenForm, correctAnswer);
        var prompt = $"Which voice command means '{fact.Meaning}'?";
        if (WouldLeakAnswer(prompt, correctAnswer))
        {
            var redacted = RedactAnswerTokens(prompt, correctAnswer);
            if (!WouldLeakAnswer(redacted, correctAnswer) && !string.IsNullOrWhiteSpace(redacted))
            {
                prompt = redacted;
            }
            else
            {
                return BuildCategoryQuestion(fact, facts);
            }
        }

        return CreateQuestion(prompt, fact.Category, fact.Source ?? "generated", correctAnswer, distractors);
    }

    private static QuizQuestion BuildCategoryQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        var correctAnswer = fact.SpokenForm;
        var distractors = GatherCategoryDistractors(fact, facts, correctAnswer);
        var article = GetIndefiniteArticle(fact.Category);
        var prompt = $"Which of the following is {article} {fact.Category}?";
        if (WouldLeakAnswer(prompt, correctAnswer))
        {
            var redacted = RedactAnswerTokens(prompt, correctAnswer);
            if (!WouldLeakAnswer(redacted, correctAnswer) && !string.IsNullOrWhiteSpace(redacted))
            {
                prompt = redacted;
            }
            else
            {
                // Fallback to a spoken->meaning question if category prompt would leak
                return BuildSpokenToMeaningQuestion(fact, facts);
            }
        }

        return CreateQuestion(prompt, fact.Category, fact.Source ?? "generated", correctAnswer, distractors);
    }

    private static string GetIndefiniteArticle(string? phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase)) return "a";
        // Trim leading whitespace and punctuation
        var trimmed = phrase.Trim();
        if (trimmed.Length == 0) return "a";
        var first = char.ToLowerInvariant(trimmed[0]);

        // Basic vowel check. Covers most English cases used in category names (Action, Modifier, etc.).
        return "aeiou".IndexOf(first) >= 0 ? "an" : "a";
    }

    private static QuizQuestion CreateQuestion(string prompt, string category, string source, string correctAnswer, IReadOnlyList<string> distractors)
    {
        var choices = new List<string> { correctAnswer };
        choices.AddRange(distractors.Where(choice => !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase)));

        if (choices.Count < 4)
        {
            choices.AddRange(Enumerable.Repeat(correctAnswer + " (review)", 4 - choices.Count).Where(choice => !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase)));
        }

        choices = choices
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(4)
            .ToList();

        if (!choices.Any(choice => string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase)))
        {
            if (choices.Count == 4)
            {
                choices[Random.Shared.Next(choices.Count)] = correctAnswer;
            }
            else
            {
                choices.Add(correctAnswer);
            }
        }

        choices = choices
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        if (choices.Count > 4)
        {
            choices = choices.Take(4).ToList();
        }

        var correctIndex = choices.FindIndex(choice => string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase));
        if (correctIndex < 0)
        {
            correctIndex = 0;
            if (choices.Count == 0)
            {
                choices.Add(correctAnswer);
            }
            else
            {
                choices[0] = correctAnswer;
            }
        }

        // Final safeguard: ensure the prompt does not contain the correct answer verbatim.
        if (WouldLeakAnswer(prompt, correctAnswer))
        {
            var redacted = RedactAnswerTokens(prompt, correctAnswer);
            // If redaction yields an empty prompt, keep the original prompt but log for diagnostics.
            if (!string.IsNullOrWhiteSpace(redacted))
            {
                prompt = redacted;
            }
            else
            {
                Debug.WriteLine($"[QuizService] Sanitization produced empty prompt for question with answer '{correctAnswer}'");
            }
        }

        return new QuizQuestion
        {
            Prompt = prompt,
            Choices = choices,
            CorrectAnswer = correctAnswer,
            CorrectIndex = correctIndex,
            Category = category,
            Source = source
        };
    }

    private static bool WouldLeakAnswer(string prompt, string correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(correctAnswer)) return false;
        try
        {
            // Case-insensitive containment check for the exact answer text.
            return Regex.IsMatch(prompt, Regex.Escape(correctAnswer), RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string RedactAnswerTokens(string prompt, string correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(correctAnswer)) return prompt;
        try
        {
            var pattern = Regex.Escape(correctAnswer);
            var redacted = Regex.Replace(prompt, pattern, "____", RegexOptions.IgnoreCase);
            return redacted;
        }
        catch
        {
            return prompt;
        }
    }

    private static List<string> GatherDistractors(QuizFact fact, IReadOnlyList<QuizFact> facts, Func<QuizFact, string> selector, string correctAnswer)
    {
        var sameCategory = facts
            .Where(candidate => !candidate.IsSameFact(fact) && string.Equals(candidate.Category, fact.Category, StringComparison.OrdinalIgnoreCase))
            .Select(selector)
            .Where(choice => !string.IsNullOrWhiteSpace(choice) && !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallback = facts
            .Where(candidate => !candidate.IsSameFact(fact))
            .Select(selector)
            .Where(choice => !string.IsNullOrWhiteSpace(choice) && !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return sameCategory
            .Concat(fallback)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();
    }

    private static List<string> GatherCategoryDistractors(QuizFact fact, IReadOnlyList<QuizFact> facts, string correctAnswer)
    {
        // For category questions ("Which of the following is a <Category>?") we want
        // distractors that are *not* in the same category as the correct answer.
        // Prefer items from other categories and fall back to same-category items
        // only if there aren't enough alternatives.
        var differentCategory = facts
            .Where(candidate => !candidate.IsSameFact(fact) && !string.Equals(candidate.Category, fact.Category, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => candidate.SpokenForm)
            .Where(choice => !string.IsNullOrWhiteSpace(choice) && !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallback = facts
            .Where(candidate => !candidate.IsSameFact(fact) && string.Equals(candidate.Category, fact.Category, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => candidate.SpokenForm)
            .Where(choice => !string.IsNullOrWhiteSpace(choice) && !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return differentCategory
            .Concat(fallback)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();
    }

}