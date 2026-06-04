using System.Text.Json;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using RazorClassLibrary.Models;
using RazorClassLibrary.Services;

namespace TestProjectxUnit;

public sealed class QuizServiceTests
{
    [Fact]
    public async Task GenerateQuestionsAsync_ReturnsQuestionsWithFourChoices()
    {
        var docsDirectory = CreateTempDocsDirectory();
        await File.WriteAllTextAsync(Path.Combine(docsDirectory, "CursorlessDocsQuizData.json"), JsonSerializer.Serialize(new[]
        {
            new CursorlessCheatsheetItemDTO { Id = 9001, SpokenForm = "blue", Meaning = "Blue hat", CursorlessType = "Hat color" },
            new CursorlessCheatsheetItemDTO { Id = 9002, SpokenForm = "green", Meaning = "Green hat", CursorlessType = "Hat color" },
            new CursorlessCheatsheetItemDTO { Id = 9003, SpokenForm = "round", Meaning = "Parentheses", CursorlessType = "Paired delimiter" },
            new CursorlessCheatsheetItemDTO { Id = 9004, SpokenForm = "box", Meaning = "Square brackets", CursorlessType = "Paired delimiter" }
        }, new JsonSerializerOptions { WriteIndented = true }));

        var service = CreateService(docsDirectory);

        var questions = await service.GenerateQuestionsAsync(4);

        Assert.Equal(4, questions.Count);
        Assert.All(questions, question =>
        {
            Assert.NotEmpty(question.Prompt);
            Assert.Equal(4, question.Choices.Count);
            Assert.InRange(question.CorrectIndex, 0, 3);
            Assert.False(string.IsNullOrWhiteSpace(question.CorrectAnswer));
            Assert.Contains(question.CorrectAnswer, question.Choices);
            Assert.False(string.IsNullOrWhiteSpace(question.Category));
            Assert.False(string.IsNullOrWhiteSpace(question.Source));
            Assert.Contains(question.Choices[question.CorrectIndex], question.Choices);
        });

        DeleteTempDirectory(docsDirectory);
    }

    [Fact]
    public async Task GenerateQuestionsAsync_FilterByCSharpPack_ReturnsOnlyCSharpQuestions()
    {
        var docsDirectory = CreateTempDocsDirectory();
        // ensure docs file exists (empty)
        await File.WriteAllTextAsync(Path.Combine(docsDirectory, "CursorlessDocsQuizData.json"), JsonSerializer.Serialize(new CursorlessCheatsheetItemDTO[0], new JsonSerializerOptions { WriteIndented = true }));

        var manualQuestions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                Prompt = "In C#, which keyword declares a namespace?",
                Choices = new List<string>{ "namespace", "using", "package", "module" },
                CorrectAnswer = "namespace",
                CorrectIndex = 0,
                Category = "CSharp",
                Source = "manual"
            },
            new QuizQuestion
            {
                Prompt = "Which keyword marks an async method in C#?",
                Choices = new List<string>{ "async", "await", "task", "thread" },
                CorrectAnswer = "async",
                CorrectIndex = 0,
                Category = "CSharp",
                Source = "manual"
            }
        };

        await File.WriteAllTextAsync(Path.Combine(docsDirectory, "CursorlessManualQuestions.json"), JsonSerializer.Serialize(manualQuestions, new JsonSerializerOptions { WriteIndented = true }));

        var service = CreateService(docsDirectory);

        var questions = await service.GenerateQuestionsAsync(2, "CSharp");

        Assert.Equal(2, questions.Count);
        Assert.All(questions, q => Assert.Equal("CSharp", q.Category));

        DeleteTempDirectory(docsDirectory);
    }

    [Fact]
    public void ScoreSerialization_RoundTrips()
    {
        var service = CreateService(CreateTempDocsDirectory());
        var scores = new List<QuizScore>
        {
            new() { PlayedAt = new DateTime(2026, 05, 11, 12, 0, 0, DateTimeKind.Utc), TotalQuestions = 10, CorrectAnswers = 8 },
            new() { PlayedAt = new DateTime(2026, 05, 11, 12, 30, 0, DateTimeKind.Utc), TotalQuestions = 20, CorrectAnswers = 17 }
        };

        var json = service.SerializeScores(scores);
        var roundTrip = service.DeserializeScores(json);

        Assert.Equal(scores.Count, roundTrip.Count);
        Assert.Equal(scores[0].CorrectAnswers, roundTrip[0].CorrectAnswers);
        Assert.Equal(scores[1].TotalQuestions, roundTrip[1].TotalQuestions);
    }

    private static QuizService CreateService(string webRootPath)
    {
        var cheatsheetService = new FakeCursorlessCheatsheetItemDataService();
        var environment = new FakeWebHostEnvironment(webRootPath);
        return new QuizService(cheatsheetService, environment);
    }

    private static string CreateTempDocsDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private sealed class FakeCursorlessCheatsheetItemDataService : ICursorlessCheatsheetItemDataService
    {
        private static readonly List<CursorlessCheatsheetItemDTO> Items = new()
        {
            new CursorlessCheatsheetItemDTO { Id = 1, SpokenForm = "chuck", Meaning = "Delete a target", CursorlessType = "Action" },
            new CursorlessCheatsheetItemDTO { Id = 2, SpokenForm = "take", Meaning = "Select a target", CursorlessType = "Action" },
            new CursorlessCheatsheetItemDTO { Id = 3, SpokenForm = "pre", Meaning = "Place cursor before a target", CursorlessType = "Action" },
            new CursorlessCheatsheetItemDTO { Id = 4, SpokenForm = "post", Meaning = "Place cursor after a target", CursorlessType = "Action" },
            new CursorlessCheatsheetItemDTO { Id = 5, SpokenForm = "line", Meaning = "Line", CursorlessType = "Scope" },
            new CursorlessCheatsheetItemDTO { Id = 6, SpokenForm = "word", Meaning = "Word", CursorlessType = "Scope" },
            new CursorlessCheatsheetItemDTO { Id = 7, SpokenForm = "round", Meaning = "Parentheses", CursorlessType = "Paired delimiter" },
            new CursorlessCheatsheetItemDTO { Id = 8, SpokenForm = "box", Meaning = "Square brackets", CursorlessType = "Paired delimiter" }
        };

        public Task<List<CursorlessCheatsheetItemDTO>> GetAllCursorlessCheatsheetItemsAsync(bool getFromJson = false) => Task.FromResult(Items.ToList());
        public Task<List<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm, bool getFromJson = false) => Task.FromResult(Items.ToList());
        public Task<CursorlessCheatsheetItemDTO?> AddCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO) => Task.FromResult<CursorlessCheatsheetItemDTO?>(cursorlessCheatsheetItemDTO);
        public Task<CursorlessCheatsheetItemDTO?> GetCursorlessCheatsheetItemById(int id) => Task.FromResult<CursorlessCheatsheetItemDTO?>(Items.FirstOrDefault(item => item.Id == id));
        public Task<CursorlessCheatsheetItemDTO?> UpdateCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO cursorlessCheatsheetItemDTO, string username) => Task.FromResult<CursorlessCheatsheetItemDTO?>(cursorlessCheatsheetItemDTO);
        public Task DeleteCursorlessCheatsheetItem(int id) => Task.CompletedTask;
        public Task<bool> ExportToJsonAsync() => Task.FromResult(true);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = webRootPath;
            WebRootFileProvider = new NullFileProvider();
            ContentRootFileProvider = new NullFileProvider();
        }

        public string ApplicationName { get; set; } = "TestProjectxUnit";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
    }
}