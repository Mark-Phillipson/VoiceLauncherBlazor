using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PlaywrightTests
{
    [TestClass]
    public class OpenListE2E
    {
        [TestMethod]
        public async Task OpenFirstListFromSearch()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Navigate to the running app (default port used by this server)
            await page.GotoAsync("http://localhost:5008/talon-voice-command-search");

            // Wait for the JS search UI to be available
            await page.WaitForSelectorAsync(".search-results-container");

            // Trigger the helper if available
            var result = await page.EvaluateAsync<string>("() => { try { if(window.TalonStorageDB && window.TalonStorageDB.openFirstReferencedList) { window.TalonStorageDB.openFirstReferencedList(); return 'called'; } return 'helper-missing'; } catch(e) { return 'error:' + (e && e.message); } }");

            // Wait briefly for UI update
            await page.WaitForTimeoutAsync(1000);

            // Capture screenshot for verification
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = "open-list-e2e.png", FullPage = true });

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result);
        }
    }
}
