using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using RazorClassLibrary.Models;

namespace TestProjectxUnit;

public sealed class AIReviewApplyTests
{
    [Fact]
    public async Task ApplyResultsToManualFile_UpdatesFile()
    {
        var webRoot = CreateTempDocsDirectory();
        var manualPath = Path.Combine(webRoot, "CursorlessManualQuestions.json");

        var questions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                Prompt = "Original prompt",
                Choices = new List<string>{ "a","b","c","d" },
                CorrectAnswer = "a",
                CorrectIndex = 0,
                Category = "cat",
                Source = "manual"
            }
        };

        await File.WriteAllTextAsync(manualPath, JsonSerializer.Serialize(questions, new JsonSerializerOptions { WriteIndented = true }));

        var env = new FakeWebHostEnvironment(webRoot);
        var logger = new NullLogger<RazorClassLibrary.Services.AIReviewApplier>();
        var applier = new RazorClassLibrary.Services.AIReviewApplier(env, null, logger);

        var results = new List<RazorClassLibrary.Models.AIReviewResult>
        {
            new RazorClassLibrary.Models.AIReviewResult { Index = 0, Ok = true, SuggestedPrompt = "Rewritten prompt", OriginalPrompt = "Original prompt" }
        };

        var applied = await applier.ApplyResultsToManualFileAsync(results);

        Assert.Equal(1, applied);

        var newJson = await File.ReadAllTextAsync(manualPath);
        var round = JsonSerializer.Deserialize<List<QuizQuestion>>(newJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Assert.Equal("Rewritten prompt", round[0].Prompt);
        Assert.True(round[0].CorrectedByAI);

        DeleteTempDirectory(webRoot);
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
