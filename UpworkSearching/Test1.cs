namespace UpworkSearching;

[TestClass]
public class Test1 : PageTest
{
    [TestMethod]
    public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingToTheIntroPage()
    {
        // . Go to Upwork job search page
        await Page.GotoAsync("https://www.upwork.com/freelance-jobs/");

        // 2. Pause for manual login/filtering
        Console.WriteLine("Please log in, solve any CAPTCHA, and set filters as needed. Press Enter to continue...");
        Console.ReadLine();

        // 3. Search for "Blazor" jobs
        await Page.FillAsync("input[placeholder='Search jobs']", "Blazor");
        await Page.PressAsync("input[placeholder='Search jobs']", "Enter");
        await Page.WaitForTimeoutAsync(3000); // Wait for results to load

        // 4. Scrape job cards
        var jobs = await Page.QuerySelectorAllAsync(".job-tile");
        foreach (var job in jobs)
        {
            var title = await job.QuerySelectorAsync(".job-title");
            if (title == null) continue;
            var titleText = await title.InnerTextAsync();
            var link = await job.QuerySelectorAsync("a");
            if (link == null) continue;
            var href = await link.GetAttributeAsync("href") ?? "";
            var rate = await job.InnerTextAsync();
            var posted = await job.InnerTextAsync();

            // 5. Filter: posted in last 3 days, $15/hr+, contains "Blazor"
            bool isBlazor = titleText.Contains("Blazor", StringComparison.OrdinalIgnoreCase);
            bool isRecent = posted.Contains("hour ago") || posted.Contains("minute ago") || (posted.Contains("day ago") && !posted.Contains("4 day"));
            bool isRateOk = rate.Contains("$15") || rate.Contains("$16") || rate.Contains("$20") || rate.Contains("$25") || rate.Contains("$30"); // Simplified, can be improved

            if (isBlazor && isRecent && isRateOk)
            {
                Console.WriteLine($"{titleText}: https://www.upwork.com{href}");
            }
        }
    }
}
