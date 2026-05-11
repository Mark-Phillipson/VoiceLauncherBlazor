using System.Text.Json;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Hosting;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public sealed class QuizService : IQuizService
{
    private const string DocsFileName = "CursorlessDocsQuizData.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ICursorlessCheatsheetItemDataService _cheatsheetDataService;
    private readonly IWebHostEnvironment _environment;

    public QuizService(ICursorlessCheatsheetItemDataService cheatsheetDataService, IWebHostEnvironment environment)
    {
        _cheatsheetDataService = cheatsheetDataService;
        _environment = environment;
    }

    public async Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<QuizQuestion>();
        }

        var facts = await LoadFactsAsync();
        if (facts.Count == 0)
        {
            return Array.Empty<QuizQuestion>();
        }

        var selectedFacts = facts
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(count, facts.Count))
            .ToList();

        var questions = selectedFacts
            .Select(fact => BuildQuestion(fact, facts))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return questions;
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
        facts.AddRange(cheatsheetItems.Select(item => QuizFact.FromItem(item, "cheatsheet")));
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
        return items.Select(item => QuizFact.FromItem(item, "docs")).ToList();
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
        return CreateQuestion($"What does '{fact.SpokenForm}' do?", fact.Category, fact.Source, correctAnswer, distractors);
    }

    private static QuizQuestion BuildMeaningToSpokenQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        var correctAnswer = fact.SpokenForm;
        var distractors = GatherDistractors(fact, facts, candidate => candidate.SpokenForm, correctAnswer);
        return CreateQuestion($"Which voice command means '{fact.Meaning}'?", fact.Category, fact.Source, correctAnswer, distractors);
    }

    private static QuizQuestion BuildCategoryQuestion(QuizFact fact, IReadOnlyList<QuizFact> facts)
    {
        var correctAnswer = fact.SpokenForm;
        var distractors = GatherCategoryDistractors(fact, facts, correctAnswer);
        return CreateQuestion($"Which of the following is a {fact.Category}?", fact.Category, fact.Source, correctAnswer, distractors);
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
        var sameCategory = facts
            .Where(candidate => !candidate.IsSameFact(fact) && string.Equals(candidate.Category, fact.Category, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => candidate.SpokenForm)
            .Where(choice => !string.IsNullOrWhiteSpace(choice) && !string.Equals(choice, correctAnswer, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var fallback = facts
            .Where(candidate => !candidate.IsSameFact(fact) && !string.Equals(candidate.Category, fact.Category, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => candidate.SpokenForm)
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

    private sealed record QuizFact(string SpokenForm, string Meaning, string Category, string Source)
    {
        public static QuizFact FromItem(CursorlessCheatsheetItemDTO item, string source)
        {
            return new QuizFact(
                item.SpokenForm.Trim(),
                item.Meaning.Trim(),
                string.IsNullOrWhiteSpace(item.CursorlessType) ? "Cursorless" : item.CursorlessType.Trim(),
                source);
        }

        public bool IsSameFact(QuizFact other)
        {
            return string.Equals(SpokenForm, other.SpokenForm, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Meaning, other.Meaning, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Category, other.Category, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Source, other.Source, StringComparison.OrdinalIgnoreCase);
        }
    }
}