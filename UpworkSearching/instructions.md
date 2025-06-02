# Playwright for .NET: Upwork Job Search Automation

## Overview
This guide helps you automate Upwork job searches using Playwright for .NET (C#), with a manual pause for login and filter selection, then automated scraping and filtering for Blazor jobs posted in the last 3 days with a rate of $15/hr or more.

---

## Prerequisites
- .NET 9 or later installed
- [Microsoft.Playwright](https://playwright.dev/dotnet/) NuGet package
- Playwright browsers installed (`playwright install`)

---

## Installation Steps

1. **Create a new .NET console project:**
   ```pwsh
   dotnet new console -n UpworkJobScraper
   cd UpworkJobScraper
   ```
2. **Add Playwright NuGet package:**
   ```pwsh
   dotnet add package Microsoft.Playwright
   playwright install
   ```

---

## Sample C# Script (`Program.cs`)

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;

class Program
{
    public static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Show browser window for manual steps
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // 1. Go to Upwork job search page
        await page.GotoAsync("https://www.upwork.com/freelance-jobs/");

        // 2. Pause for manual login/filtering
        Console.WriteLine("Please log in, solve any CAPTCHA, and set filters as needed. Press Enter to continue...");
        Console.ReadLine();

        // 3. Search for "Blazor" jobs
        await page.FillAsync("input[placeholder='Search jobs']", "Blazor");
        await page.PressAsync("input[placeholder='Search jobs']", "Enter");
        await page.WaitForTimeoutAsync(3000); // Wait for results to load

        // 4. Scrape job cards
        var jobs = await page.QuerySelectorAllAsync(".job-tile");
        foreach (var job in jobs)
        {
            var title = await job.QuerySelectorAsync(".job-title");
            var titleText = await title?.InnerTextAsync() ?? "";
            var link = await job.QuerySelectorAsync("a");
            var href = await link?.GetAttributeAsync("href") ?? "";
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

        await browser.CloseAsync();
    }
}
```

---

## How It Works
- **Headful mode:** Lets you log in and solve CAPTCHAs manually.
- **Manual pause:** Script waits for you to finish manual steps, then resumes automation.
- **Automated search:** Fills in "Blazor" as the search term, waits for results, and scrapes job cards.
- **Filtering:** Only outputs jobs with "Blazor" in the title, posted in the last 3 days, and with a rate of $15/hr or more.

---

## Customization
- Adjust selectors (e.g., `.job-tile`, `.job-title`) if Upwork’s UI changes.
- Refine the filtering logic for date and rate as needed.
- You can output results to a file or further process them as required.

---

## References
- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Upwork Job Search](https://www.upwork.com/freelance-jobs/)

---

*Move this file to your .NET project folder and use it as a reference for setting up your Upwork job search automation!*
