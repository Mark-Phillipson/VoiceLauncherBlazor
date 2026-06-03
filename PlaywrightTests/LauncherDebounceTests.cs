using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LauncherDebounceTests : PageTest
{
    // Read base URL from env `PLAYWRIGHT_BASEURL`, default to local dev server
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASEURL") ?? "http://localhost:5000";

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync(BaseUrl + "/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
        try { await Page.SetViewportSizeAsync(1280, 900); } catch { }

        var navToggler = Page.Locator("button[aria-label=\"Toggle Navigation Menu\"]");
        if (await navToggler.IsVisibleAsync())
        {
            await navToggler.ClickAsync();
        }
    }

    [Test]
    public async Task Favorites_Filter_DoesNotOpenMultiplePages()
    {
        var before = Page.Context.Pages.Count;
        await Page.GotoAsync(BaseUrl + "/launchersfavourites/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
        await Page.WaitForSelectorAsync("#SearchInput", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        await Page.FocusAsync("#SearchInput");
        // Type a few characters quickly
        await Page.Keyboard.TypeAsync("test");
        // Wait for debounce to elapse and any potential windows to open
        await Task.Delay(900);

        var after = Page.Context.Pages.Count;
        Assert.LessOrEqual(after - before, 1, $"More than one new page opened while filtering favorites (opened {after - before}).");
    }

    [Test]
    public async Task Launcher_Filter_DoesNotOpenMultiplePages()
    {
        var before = Page.Context.Pages.Count;
        await Page.GotoAsync(BaseUrl + "/launcherstable/1", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
        await Page.WaitForSelectorAsync("#SearchInput", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        await Page.FocusAsync("#SearchInput");
        await Page.Keyboard.TypeAsync("test");
        await Task.Delay(900);

        var after = Page.Context.Pages.Count;
        Assert.LessOrEqual(after - before, 1, $"More than one new page opened while filtering launcher (opened {after - before}).");
    }
}
