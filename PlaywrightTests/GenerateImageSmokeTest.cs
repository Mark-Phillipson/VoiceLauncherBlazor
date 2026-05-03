using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System;

namespace PlaywrightTests;

[TestClass]
public class GenerateImageSmokeTest
{
    private const string BaseUrl = "http://localhost:5008";

    [TestMethod]
    public async Task GenerateImage_ButtonCreatesImage_Test()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 1366, Height = 900 } });
        var page = await context.NewPageAsync();

        var url = BaseUrl + "/launcheradd";
        Console.WriteLine($"Navigating to {url}");
        await page.GotoAsync(url, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });

        // Wait for Generate Image button
        var genButton = await page.QuerySelectorAsync("button:has-text('Generate Image')");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(genButton, "Generate Image button should exist on the page");

        // Click the button
        await genButton.ClickAsync();

        // Wait for a generated image entry to appear in the images list
        try
        {
            await page.WaitForSelectorAsync("img[src^='/images/launcher-']", new() { Timeout = 15000 });
            Console.WriteLine("Generated image appeared in images list");
        }
        catch (TimeoutException)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Generated image did not appear within timeout");
        }

        await browser.CloseAsync();
    }
}
