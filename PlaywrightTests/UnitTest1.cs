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
    public async Task CheckTalonLink()
    {
        await Page.GotoAsync("http://localhost:5000/");

        await Page.Locator("a").Filter(new() { HasText = "Launch Categories [£]" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Talon (8)" }).ClickAsync();

        var Page1 = await Page.RunAndWaitForPopupAsync(async () =>
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "I Talon Main Website" }).ClickAsync();
        });

        await Expect(Page.GetByText("Talon").First).ToBeVisibleAsync();

    }




    //dotnet test --filter "Name~CheckTalonLink" -- Playwright.LaunchOptions.Headless=false
}