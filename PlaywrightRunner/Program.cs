using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Playwright runner starting...");
    using var playwright = await Playwright.CreateAsync();

    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1365, Height = 768 }
        });
        var page = await context.NewPageAsync();

        var url = "http://localhost:5269/talon-voice-command-search";
        Console.WriteLine($"Waiting for {url} to be available...");

        // Wait up to 30s for the server to be ready
    using var http = new System.Net.Http.HttpClient();
        var ready = false;
        for (int i = 0; i < 30; i++)
        {
            try
            {
                var resp = await http.GetAsync("http://localhost:5269/");
                if (resp.IsSuccessStatusCode)
                {
                    ready = true;
                    break;
                }
            }
            catch { }
            await Task.Delay(1000);
        }

        if (!ready)
        {
            Console.WriteLine("Server did not become ready in time.");
            return 2;
        }

        Console.WriteLine($"Navigating to {url}");
        await page.GotoAsync(url, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });

        var beforePath = System.IO.Path.Combine(Environment.CurrentDirectory, "playwright-before.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = beforePath, FullPage = true });
        Console.WriteLine($"Saved before screenshot: {beforePath}");

        Console.WriteLine("Reloading page...");
        await page.ReloadAsync(new PageReloadOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });
        var afterPath = System.IO.Path.Combine(Environment.CurrentDirectory, "playwright-after.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = afterPath, FullPage = true });
        Console.WriteLine($"Saved after screenshot: {afterPath}");

        await browser.CloseAsync();
        return 0;
    }
}
