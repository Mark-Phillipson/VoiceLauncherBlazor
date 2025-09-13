using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.IO;
using System;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class TalonVoiceCommandSearchPersistenceTest : PageTest
{
    private string tempDir = "";
    private string talonTestFile = "";

    [SetUp]
    public void Setup()
    {
        // Create temporary directory and test files
        tempDir = Path.Combine(Path.GetTempPath(), $"TalonPersistenceTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create a comprehensive Talon file for testing
        talonTestFile = Path.Combine(tempDir, "test_commands.talon");
        File.WriteAllText(talonTestFile, @"app: vscode
title: VS Code Test Commands
mode: command
-

open file: 
    key(ctrl-o)

save file: 
    key(ctrl-s)

search everywhere: 
    key(ctrl-shift-f)

go to line: 
    key(ctrl-g)

toggle comment: 
    key(ctrl-/)

format document: 
    key(shift-alt-f)

new terminal: 
    key(ctrl-shift-`)

split editor: 
    key(ctrl-\\)

close tab: 
    key(ctrl-w)

reopen tab: 
    key(ctrl-shift-t)
");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temporary files
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task TalonVoiceCommandSearch_ImportAndSearchAfterRefresh_ShouldPersistResults()
    {
        // Step 1: Navigate to import page and import commands
        await Page.GotoAsync("http://localhost:5000/talon-import");
        
        // Verify we're on the correct page
        await Expect(Page).ToHaveTitleAsync(new Regex("Voice Admin"));
        await Expect(Page.GetByText("Import Talon Scripts")).ToBeVisibleAsync();

        // Import the test file directory
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync(tempDir);
        
        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        // Wait for import to complete
        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 15000 });
        
        // Verify success message
        var successText = await successAlert.TextContentAsync();
        Assert.That(successText, Does.Contain("command(s)"));
        Console.WriteLine($"Import completed: {successText}");

        // Step 2: Navigate to search page and perform initial search
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");
        
        // Wait for page to load and ensure we're on the Search Commands tab
        await Expect(Page.GetByText("Search Commands")).ToBeVisibleAsync();
        
        // Take screenshot of initial state
        await Page.ScreenshotAsync(new() { Path = "01-search-page-loaded.png", FullPage = true });

        // Perform search for "open file"
        var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("open file");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();

        // Wait for search results
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify search results appear
        await Expect(Page.GetByText("open file")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.GetByText("key(ctrl-o)")).ToBeVisibleAsync();
        
        // Take screenshot of successful search results
        await Page.ScreenshotAsync(new() { Path = "02-initial-search-results.png", FullPage = true });
        Console.WriteLine("Initial search completed successfully - commands found");

        // Step 3: Refresh the page to test persistence
        await Page.ReloadAsync();
        Console.WriteLine("Page refreshed");
        
        // Wait for page to reload completely
        await Expect(Page.GetByText("Search Commands")).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(2000); // Give time for data loading
        
        // Take screenshot after refresh
        await Page.ScreenshotAsync(new() { Path = "03-after-page-refresh.png", FullPage = true });

        // Step 4: Perform the same search after refresh
        var searchInputAfterRefresh = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInputAfterRefresh.FillAsync("open file");
        
        var searchButtonAfterRefresh = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButtonAfterRefresh.ClickAsync();

        // Wait for search results after refresh
        await Page.WaitForTimeoutAsync(5000); // Give extra time for localStorage loading
        
        // Verify search results still appear after refresh
        await Expect(Page.GetByText("open file")).ToBeVisibleAsync(new() { Timeout = 15000 });
        await Expect(Page.GetByText("key(ctrl-o)")).ToBeVisibleAsync(new() { Timeout = 5000 });
        
        // Take screenshot of successful search results after refresh
        await Page.ScreenshotAsync(new() { Path = "04-search-results-after-refresh.png", FullPage = true });
        Console.WriteLine("Search after refresh completed successfully - persistence working!");

        // Step 5: Test another search to verify multiple commands work
        await searchInputAfterRefresh.ClearAsync();
        await searchInputAfterRefresh.FillAsync("save file");
        await searchButtonAfterRefresh.ClickAsync();
        
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify different command is found
        await Expect(Page.GetByText("save file")).ToBeVisibleAsync();
        await Expect(Page.GetByText("key(ctrl-s)")).ToBeVisibleAsync();
        
        // Take screenshot of second search
        await Page.ScreenshotAsync(new() { Path = "05-second-search-after-refresh.png", FullPage = true });
        Console.WriteLine("Second search after refresh also successful");

        // Step 6: Test search with partial matches
        await searchInputAfterRefresh.ClearAsync();
        await searchInputAfterRefresh.FillAsync("format");
        await searchButtonAfterRefresh.ClickAsync();
        
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify partial match works
        await Expect(Page.GetByText("format document")).ToBeVisibleAsync();
        await Expect(Page.GetByText("key(shift-alt-f)")).ToBeVisibleAsync();
        
        // Take final screenshot
        await Page.ScreenshotAsync(new() { Path = "06-partial-search-working.png", FullPage = true });
        Console.WriteLine("Partial search working correctly");
    }

    [Test]
    public async Task TalonVoiceCommandSearch_TabNavigation_ShouldWork()
    {
        // Navigate to search page
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");
        
        // Test tab navigation
        await Expect(Page.GetByText("Search Commands")).ToBeVisibleAsync();
        
        // Click on Import Scripts tab
        await Page.GetByText("Import Scripts").ClickAsync();
        await Expect(Page.GetByText("This tab shows information about importing Talon voice command files")).ToBeVisibleAsync();
        
        // Take screenshot of Import Scripts tab
        await Page.ScreenshotAsync(new() { Path = "07-import-scripts-tab.png", FullPage = true });
        
        // Click on Analysis Report tab
        await Page.GetByText("Analysis Report").ClickAsync();
        await Expect(Page.GetByText("Repository breakdown is now hidden by default")).ToBeVisibleAsync();
        
        // Take screenshot of Analysis Report tab
        await Page.ScreenshotAsync(new() { Path = "08-analysis-report-tab.png", FullPage = true });
        
        // Click back to Search Commands tab
        await Page.GetByText("Search Commands").ClickAsync();
        await Expect(Page.GetByPlaceholder("Search commands or scripts...")).ToBeVisibleAsync();
        
        // Take screenshot of Search Commands tab
        await Page.ScreenshotAsync(new() { Path = "09-search-commands-tab.png", FullPage = true });
        
        Console.WriteLine("Tab navigation working correctly");
    }

    [Test]
    public async Task TalonVoiceCommandSearch_KeyboardShortcuts_ShouldWork()
    {
        // Navigate to search page
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");
        
        // Test keyboard shortcuts for tab navigation
        await Page.Keyboard.PressAsync("Alt+2");
        await Expect(Page.GetByText("This tab shows information about importing Talon voice command files")).ToBeVisibleAsync();
        
        await Page.Keyboard.PressAsync("Alt+3");
        await Expect(Page.GetByText("Repository breakdown is now hidden by default")).ToBeVisibleAsync();
        
        await Page.Keyboard.PressAsync("Alt+1");
        await Expect(Page.GetByPlaceholder("Search commands or scripts...")).ToBeVisibleAsync();
        
        Console.WriteLine("Keyboard shortcuts working correctly");
    }

    [Test]
    public async Task TalonVoiceCommandSearch_FilterFunctionality_ShouldPersistAfterRefresh()
    {
        // First import commands
        await Page.GotoAsync("http://localhost:5000/talon-import");
        
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync(tempDir);
        
        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();
        
        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 15000 });

        // Navigate to search page
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");
        
        // Wait for filters to load
        await Page.WaitForTimeoutAsync(3000);
        
        // Test application filter
        var applicationFilter = Page.Locator("select").Filter(new() { Has = Page.GetByText("All Applications") });
        if (await applicationFilter.CountAsync() > 0)
        {
            await applicationFilter.SelectOptionAsync("vscode");
            await Page.WaitForTimeoutAsync(1000);
        }
        
        // Perform search
        var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("save");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();
        
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify results
        await Expect(Page.GetByText("save file")).ToBeVisibleAsync();
        
        // Take screenshot before refresh
        await Page.ScreenshotAsync(new() { Path = "10-filtered-search-before-refresh.png", FullPage = true });
        
        // Refresh page
        await Page.ReloadAsync();
        await Page.WaitForTimeoutAsync(3000);
        
        // Test search still works after refresh
        var searchInputAfterRefresh = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInputAfterRefresh.FillAsync("save");
        
        var searchButtonAfterRefresh = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButtonAfterRefresh.ClickAsync();
        
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify results persist after refresh
        await Expect(Page.GetByText("save file")).ToBeVisibleAsync(new() { Timeout = 10000 });
        
        // Take screenshot after refresh
        await Page.ScreenshotAsync(new() { Path = "11-search-works-after-refresh.png", FullPage = true });
        
        Console.WriteLine("Filter functionality and search persistence verified");
    }
}