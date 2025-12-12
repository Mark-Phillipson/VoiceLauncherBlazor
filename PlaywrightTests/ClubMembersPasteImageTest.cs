using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System;
using System.IO;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ClubMembersPasteImageTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            Permissions = new[] { "clipboard-read", "clipboard-write" }
        };
    }

    [Test]
    public async Task PasteImageAndSave_ShouldShowImageInMemberCard()
    {
        Page.Console += (_, msg) => Console.WriteLine($"Browser Console: {msg.Text}");
        var url = "http://127.0.0.1:5008/club-members";
        await Page.GotoAsync(url);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByPlaceholder("First name").FillAsync("PlaywrightTestFirst");
        await Page.GetByPlaceholder("Last name").FillAsync("PlaywrightTestLast");

        // Stub the clipboard helper to return a data URL when the Paste button is used
        var b64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVQImWNgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII=";
        await Page.EvaluateAsync(@"(b64) => {
            window.blazorClipboard = window.blazorClipboard || {};
            window.blazorClipboard.getImageFromClipboard = async () => {
                return 'data:image/png;base64,' + b64;
            };
        }", b64);

        // Trigger the paste via the new button (user gesture)
        await Page.GetByRole(AriaRole.Button, new() { Name = "Paste Image" }).ClickAsync();

        await Expect(Page.GetByText("Image pasted!")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Member" }).ClickAsync();

        var cardLocator = Page.Locator(".card").Filter(new() { HasText = "PlaywrightTestFirst PlaywrightTestLast" });
        await Expect(cardLocator).ToBeVisibleAsync(new() { Timeout = 10000 });

        var img = cardLocator.Locator("img");
        await Expect(img).ToBeVisibleAsync(new() { Timeout = 5000 });

        var src = await img.GetAttributeAsync("src");
        Assert.That(src, Does.StartWith("data:"));
    }

    [Test]
    [Explicit("Use for visual debugging")]
    public async Task PasteImageAndSave_Headed_Debug()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 1365, Height = 768 } });
        var page = await context.NewPageAsync();

        var url = "http://localhost:5008/club-members";
        await page.GotoAsync(url, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByPlaceholder("First name").FillAsync("PlaywrightHeadedFirst");
        await page.GetByPlaceholder("Last name").FillAsync("PlaywrightHeadedLast");

        await page.FocusAsync("#paste-target");
        
        var b64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVQImWNgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII=";
        await page.EvaluateAsync(@"(b64) => {
            const raw = atob(b64);
            const arr = new Uint8Array(raw.length);
            for (let i = 0; i < raw.length; ++i) arr[i] = raw.charCodeAt(i);
            const blob = new Blob([arr], { type: 'image/png' });
            const file = new File([blob], 'paste.png', { type: 'image/png' });
            const data = new DataTransfer();
            data.items.add(file);
            const evt = new ClipboardEvent('paste', { clipboardData: data, bubbles: true });
            const el = document.getElementById('paste-target') || document;
            el.dispatchEvent(evt);
        }", b64);

        await page.WaitForTimeoutAsync(1000);
        await page.GetByRole(AriaRole.Button, new() { Name = "Save Member" }).ClickAsync();
        await page.WaitForTimeoutAsync(2000);
        
        await browser.CloseAsync();
    }
}
