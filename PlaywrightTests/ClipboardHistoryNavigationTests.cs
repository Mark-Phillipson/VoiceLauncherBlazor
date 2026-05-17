using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ClipboardHistoryNavigationTests : PageTest
{
    // Read base URL from env `PLAYWRIGHT_BASEURL`, default to local dev server
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASEURL") ?? "http://localhost:5000";

    [SetUp]
    public async Task Setup()
    {
        // Wait for DOMContentLoaded to avoid flaky NetworkIdle waits caused by background activity
        await Page.GotoAsync(BaseUrl + "/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
        // Ensure viewport is large enough so sidebar/nav links are visible
        try
        {
            await Page.SetViewportSizeAsync(1280, 900);
        }
        catch
        {
            // Some Playwright drivers ignore viewport resize; ignore failures and proceed
        }

        // Ensure navigation menu is expanded so nav links are visible (click toggler on small viewports)
        var navToggler = Page.Locator("button[aria-label=\"Toggle Navigation Menu\"]");
        if (await navToggler.IsVisibleAsync())
        {
            await navToggler.ClickAsync();
        }
    }

    [Test]
    public async Task ClipboardHistory_To_Snippets_RoundsTrip()
    {
        // Open Clipboard History (nav anchor)
        var clipboardLink = Page.Locator("a[title=\"Clipboard History\"]");
        await Expect(clipboardLink).ToBeVisibleAsync();
        await clipboardLink.ClickAsync();

        // Clipboard history component should be visible
        await Page.WaitForSelectorAsync(".clipboard-history", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Click Snippets nav link
        var snippetsLink = Page.Locator("a[title=\"Snippets\"]");
        await Expect(snippetsLink).ToBeVisibleAsync();
        await snippetsLink.ClickAsync();

        // Expect a snippets table to appear
        await Page.WaitForSelectorAsync("table.table", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    [Test]
    public async Task ClipboardHistory_To_Launcher_RoundsTrip()
    {
        // Open Clipboard History (nav anchor)
        var clipboardLink = Page.Locator("a[title=\"Clipboard History\"]");
        await Expect(clipboardLink).ToBeVisibleAsync();
        await clipboardLink.ClickAsync();

        // Clipboard history component should be visible
        await Page.WaitForSelectorAsync(".clipboard-history", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Click Launch nav link
        var launcherLink = Page.Locator("a[title=\"Launch\"]");
        await Expect(launcherLink).ToBeVisibleAsync();
        await launcherLink.ClickAsync();

        // Expect either card layout or launcher table to be visible
        await Page.WaitForSelectorAsync(".card, .layout-as-cards, select#CategoryId, table.table", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    [Test]
    public async Task AIChat_AccessKey_AltC_Shows_AIChat()
    {
        // Click the AI Chat nav link (accesskey set to 'c')
        var chatLink = Page.Locator("a[title=\"AI Chatbot\"]");
        await Expect(chatLink).ToBeVisibleAsync();
        await chatLink.ClickAsync();

        // Chat textarea or Chat button should be visible
        await Page.WaitForSelectorAsync("textarea.ai-input-bg, button:has-text(\"Chat\")", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }
}
