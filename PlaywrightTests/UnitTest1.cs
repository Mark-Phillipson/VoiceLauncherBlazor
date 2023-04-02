using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    [Test]
    public async Task CheckHomepageHasVoiceAdminTitle()
    {
        await Page.GotoAsync("http://localhost:5000/");

        // Expect a title "to contain" a substring.
        await Expect(Page).ToHaveTitleAsync(new Regex("Voice Admin"));

        // create a locator
        //var getStarted = Page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

        // Expect an attribute "to be strictly equal" to the value.
        //await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

        // Click the get started link.
        //await getStarted.ClickAsync();

        // Expects the URL to contain intro.
        //await Expect(Page).ToHaveURLAsync(new Regex(".*intro"));
    }

    [Test]
    public async Task VoiceAdminIntoBlazorSite()
    {
        await Page.PauseAsync();
        await Page.GotoAsync("http://localhost:5000/");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Launcher Categories" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Learning" }).ClickAsync();

        var page2 = await Page.RunAndWaitForPopupAsync(async () =>
        {
            await Page.GetByRole(AriaRole.Row, new() { Name = " Blazor Site https://Blazor.net " }).GetByRole(AriaRole.Link, new() { Name = "" }).First.ClickAsync();
        });
        await Expect(page2).ToHaveTitleAsync(new Regex("Build client web apps"));
    }
    //Create a method test using playwright to check the homepage Has a h1 title of Voice Admin




    //dotnet test --filter "Name~VoiceAdminIntoBlazorSite" -- Playwright.LaunchOptions.Headless=false
}