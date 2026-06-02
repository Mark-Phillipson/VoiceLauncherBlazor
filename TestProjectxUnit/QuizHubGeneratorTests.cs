using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bunit;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using RazorClassLibrary.Models;
using RazorClassLibrary.Pages;
using TestProjectxUnit.TestStubs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TestProjectxUnit
{
    public class QuizHubGeneratorTests : TestContext
    {
        public QuizHubGeneratorTests()
        {
            // Provide minimal services used by the component in tests
            Services.AddSingleton<RazorClassLibrary.Services.IWindowsService>(new WindowsServiceStub());
            Services.AddSingleton<RazorClassLibrary.Services.ApplicationMappingService>();
            // Minimal AI review service stub to satisfy component DI
            Services.AddSingleton<RazorClassLibrary.Services.IAIReviewService>(new TestAiReviewServiceStub());
            // Minimal quiz and talon list services
            Services.AddSingleton<RazorClassLibrary.Services.IQuizService>(new TestQuizServiceStub());
            Services.AddSingleton<RazorClassLibrary.Services.ITalonListDataService>(new TestTalonListDataService());
            JsInteropStubs.ConfigureSelectionModalInterop(this);
        }

        private TalonVoiceCommand CreateCmd(int id, string command, string? title = null, string? script = null, string? description = null, string? application = "global")
        {
            return new TalonVoiceCommand
            {
                Id = id,
                Command = command,
                Title = title,
                Script = script ?? "user.noop()",
                Application = application ?? "global",
                Description = description,
                FilePath = $"file{id}.talon",
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task GenerateTalonVoiceCommandQuestionsAsync_UsesFullPool_WhenPackNameEmpty()
        {
            var stub = new StubTalonVoiceCommandDataService();
            var commands = new[]
            {
                CreateCmd(1, "open file", title: "Title1"),
                CreateCmd(2, "save file", script: "user.save_file()"),
                CreateCmd(3, "copy", description: "Copy selection")
            };
            stub.SetCommands(commands);
            Services.AddSingleton<ITalonVoiceCommandDataService>(stub);

            var comp = Render<QuizHub>();

            // Inject the prepared command list into the component's private field
            var field = comp.Instance.GetType().GetField("_allVoiceCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(comp.Instance, commands.ToList());

            var method = comp.Instance.GetType().GetMethod("GenerateTalonVoiceCommandQuestionsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var invoked = method.Invoke(comp.Instance, new object[] { 3, "" });
            var task = (Task)invoked!;
            await task;
            var result = (IReadOnlyList<QuizQuestion>)invoked.GetType().GetProperty("Result")!.GetValue(invoked)!;

            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result, q => Assert.Equal("talon-voice-command", q.Source));
        }

        [Fact]
        public async Task GenerateTalonVoiceCommandQuestionsAsync_ExcludesCommandsWithoutTitleScriptOrDescription()
        {
            var stub = new StubTalonVoiceCommandDataService();
            var good = CreateCmd(10, "good", title: "t1");
            var bad = CreateCmd(11, "bad"); // no title/script/description
            var commands = new[] { good, bad, CreateCmd(12, "also good", script: "s") };
            stub.SetCommands(commands);
            Services.AddSingleton<ITalonVoiceCommandDataService>(stub);

            var comp = Render<QuizHub>();
            var field = comp.Instance.GetType().GetField("_allVoiceCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(comp.Instance, commands.ToList());

            var method = comp.Instance.GetType().GetMethod("GenerateTalonVoiceCommandQuestionsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var invoked = method.Invoke(comp.Instance, new object[] { 5, null });
            var task = (Task)invoked!;
            await task;
            var result = (IReadOnlyList<QuizQuestion>)invoked.GetType().GetProperty("Result")!.GetValue(invoked)!;

            // ensure no question's RelatedCommandId equals bad.Id
            Assert.DoesNotContain(result, q => q.RelatedCommandId == bad.Id);
        }

        [Fact]
        public async Task GenerateTalonVoiceCommandQuestionsAsync_DedupesCommandsByNormalizedKey()
        {
            var stub = new StubTalonVoiceCommandDataService();
            var c1 = CreateCmd(20, "Open File", title: "t1");
            var c2 = CreateCmd(21, "\"open file\"", title: "t2"); // normalized same key
            var others = CreateCmd(22, "save", title: "t3");
            var commands = new[] { c1, c2, others };
            stub.SetCommands(commands);
            Services.AddSingleton<ITalonVoiceCommandDataService>(stub);

            var comp = Render<QuizHub>();
            var field = comp.Instance.GetType().GetField("_allVoiceCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(comp.Instance, commands.ToList());

            var method = comp.Instance.GetType().GetMethod("GenerateTalonVoiceCommandQuestionsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var invoked = method.Invoke(comp.Instance, new object[] { 10, null });
            var task = (Task)invoked!;
            await task;
            var result = (IReadOnlyList<QuizQuestion>)invoked.GetType().GetProperty("Result")!.GetValue(invoked)!;

            // since Open File and "open file" normalize to the same key, only one should be represented
            var relatedIds = result.Select(q => q.RelatedCommandId).ToList();
            Assert.Contains(20, relatedIds);
            Assert.DoesNotContain(21, relatedIds);
        }
    }

    // Minimal stub implementation for IAIReviewService used in component tests
    internal sealed class TestAiReviewServiceStub : RazorClassLibrary.Services.IAIReviewService
    {
        private readonly System.Threading.Channels.Channel<QuizQuestion[]> _ch = System.Threading.Channels.Channel.CreateUnbounded<QuizQuestion[]>();

        public Task EnqueueForReviewAsync(IEnumerable<QuizQuestion> questions, System.Threading.CancellationToken ct = default)
        {
            _ch.Writer.TryWrite(questions.ToArray());
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RazorClassLibrary.Models.AIReviewResult>> ReviewQuestionsAsync(IEnumerable<QuizQuestion> questions, System.Threading.CancellationToken ct = default)
        {
            return Task.FromResult((IReadOnlyList<RazorClassLibrary.Models.AIReviewResult>)Array.Empty<RazorClassLibrary.Models.AIReviewResult>());
        }

        public System.Threading.Channels.ChannelReader<QuizQuestion[]> GetReviewReader() => _ch.Reader;
    }

    internal sealed class TestQuizServiceStub : RazorClassLibrary.Services.IQuizService
    {
        public Task<IReadOnlyList<QuizQuestion>> GenerateQuestionsAsync(int count) => Task.FromResult((IReadOnlyList<QuizQuestion>)Array.Empty<QuizQuestion>());
        public List<RazorClassLibrary.Models.QuizScore> DeserializeScores(string? json) => new List<RazorClassLibrary.Models.QuizScore>();
        public string SerializeScores(IEnumerable<RazorClassLibrary.Models.QuizScore> scores) => string.Empty;
    }

    internal sealed class TestTalonListDataService : RazorClassLibrary.Services.ITalonListDataService
    {
        public Task<IEnumerable<DataAccessLibrary.Models.TalonList>> GetAllTalonListsAsync()
        {
            return Task.FromResult(Enumerable.Empty<DataAccessLibrary.Models.TalonList>());
        }
    }
}
