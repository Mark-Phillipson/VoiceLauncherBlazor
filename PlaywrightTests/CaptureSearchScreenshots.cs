using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightTests;

[TestClass]
public class CaptureSearchScreenshots
{
    [TestMethod]
    public async Task CaptureBeforeAndAfterRefresh()
    {
        // Create Playwright
        using var playwright = await Playwright.CreateAsync();

        // Launch browser (headless). If you want to see the browser, set Headless = false
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1365, Height = 768 }
        });
        var page = await context.NewPageAsync();

        var url = "http://localhost:5269/talon-voice-command-search";
    System.Console.WriteLine($"Navigating to {url}");

        // Navigate and wait for network idle
        await page.GotoAsync(url, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });

        // Capture before refresh
        var beforePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "playwright-before.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = beforePath, FullPage = true });
    System.Console.WriteLine($"Saved before screenshot: {beforePath}");

        // Refresh the page and capture after
        await page.ReloadAsync(new PageReloadOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });
        var afterPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "playwright-after.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = afterPath, FullPage = true });
    System.Console.WriteLine($"Saved after screenshot: {afterPath}");

        await browser.CloseAsync();
    }

    public Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? TestContext { get; set; }
}
