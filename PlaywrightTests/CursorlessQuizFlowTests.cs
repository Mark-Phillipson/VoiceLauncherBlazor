using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CursorlessQuizFlowTests : PageTest
{
    private const string BaseUrl = "http://localhost:5008";
    private const string QuizPath = "/cursorless-quiz";

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseUrl}{QuizPath}", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Page.WaitForSelectorAsync("h1.quiz-title", new() { State = WaitForSelectorState.Visible, Timeout = 30000 });
    }

    [Test]
    public async Task Quiz_Should_Progress_For_Up_To_Ten_Questions()
    {
        var startButton = Page.GetByRole(AriaRole.Button, new() { Name = "Start quiz" });
        await Expect(startButton).ToBeVisibleAsync();
        await startButton.ClickAsync();

        var badge = Page.Locator(".quiz-badge");
        await Expect(badge).ToContainTextAsync("Question 1 /");

        var maxQuestionsToVerify = 10;
        for (var questionNumber = 1; questionNumber <= maxQuestionsToVerify; questionNumber++)
        {
            var expectedCurrentPattern = new Regex($"Question\\s+{questionNumber}\\s*/", RegexOptions.IgnoreCase);
            await Expect(badge).ToContainTextAsync(expectedCurrentPattern);

            var firstChoice = Page.Locator("button.quiz-choice").First;
            await Expect(firstChoice).ToBeVisibleAsync();
            await firstChoice.ClickAsync();

            var nextQuestionPattern = new Regex($"Question\\s+{questionNumber + 1}\\s*/", RegexOptions.IgnoreCase);
            var feedback = Page.Locator(".quiz-feedback");

            // Correct path auto-advances; incorrect path shows feedback + requires Next.
            var advancedAutomatically = await BadgeMatchesAsync(badge, nextQuestionPattern, 1800);
            if (!advancedAutomatically)
            {
                await Expect(feedback).ToBeVisibleAsync(new() { Timeout = 3000 });
                var nextButton = Page.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true });
                await Expect(nextButton).ToBeVisibleAsync();
                await nextButton.ClickAsync();
            }

            if (questionNumber < maxQuestionsToVerify)
            {
                await Expect(badge).ToContainTextAsync(nextQuestionPattern);
            }
        }
    }

    private static async Task<bool> BadgeMatchesAsync(ILocator badge, Regex pattern, int timeoutMs)
    {
        var started = DateTime.UtcNow;
        while ((DateTime.UtcNow - started).TotalMilliseconds < timeoutMs)
        {
            var text = await badge.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text) && pattern.IsMatch(text))
            {
                return true;
            }

            await Task.Delay(120);
        }

        return false;
    }
}
