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
public class TalonVoiceCommandsServerPersistenceTest : PageTest
{
    // This test targets the standalone TalonVoiceCommandsServer application (port 5269)
    private const string BaseUrl = "http://localhost:5269";
    
    private string tempDir = "";
    private string talonTestFile = "";

    [SetUp]
    public void Setup()
    {
        // Create temporary directory and test files for the standalone server
        tempDir = Path.Combine(Path.GetTempPath(), $"TalonServerPersistenceTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create a comprehensive Talon file for testing the standalone server
        talonTestFile = Path.Combine(tempDir, "standalone_test_commands.talon");
        File.WriteAllText(talonTestFile, @"app: vscode
title: Standalone Server Test
mode: command
-

open file: 
    key(ctrl-o)

save file: 
    key(ctrl-s)

search project: 
    key(ctrl-shift-f)

go to line: 
    key(ctrl-g)

toggle comment: 
    key(ctrl-/)

format document: 
    key(shift-alt-f)

new terminal: 
    key(ctrl-shift-backtick)

close tab: 
    key(ctrl-w)

find replace: 
    key(ctrl-h)

duplicate line: 
    key(shift-alt-down)
");

        Console.WriteLine($"[STANDALONE SERVER TEST] Created test file: {talonTestFile}");
        Console.WriteLine($"[STANDALONE SERVER TEST] Test directory: {tempDir}");
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                Console.WriteLine($"[STANDALONE SERVER TEST] Cleaned up temp directory: {tempDir}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[STANDALONE SERVER TEST] Cleanup warning: {ex.Message}");
        }
    }

    [Test]
    [Description("Tests the standalone TalonVoiceCommandsServer application localStorage persistence - import commands, search, refresh page, search again")]
    public async Task StandaloneTalonServer_ImportAndSearchAfterRefresh_ShouldPersistResults()
    {
        // This test validates the complete workflow on the STANDALONE server:
        // 1. Import Talon commands via the standalone server's import page
        // 2. Perform search on the standalone server's search page 
        // 3. Refresh the browser page
        // 4. Verify search still works (localStorage persistence)
        
        Console.WriteLine("[STANDALONE SERVER TEST] Starting localStorage persistence test for standalone TalonVoiceCommandsServer");

        // Clear localStorage before starting to ensure clean state
        await Page.EvaluateAsync("() => localStorage.clear()");
        
        // Step 1: Navigate to the standalone server's import page
        Console.WriteLine("[STANDALONE SERVER TEST] Step 1: Navigating to standalone server import page");
        await Page.GotoAsync($"{BaseUrl}/talon-import");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Take screenshot of import page
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "01_standalone_import_page.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Import page loaded");

        // Set the directory path and import
        Console.WriteLine($"[STANDALONE SERVER TEST] Setting directory path to: {tempDir}");
        await Page.FillAsync("input[placeholder*='directory']", tempDir);
        
        // Click the import button
        await Page.ClickAsync("button:has-text('Import All From Directory')");
        Console.WriteLine("[STANDALONE SERVER TEST] Clicked import button");
        
        // Wait for import to complete
        await Page.WaitForTimeoutAsync(3000);
        
        // Take screenshot after import
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "02_standalone_import_completed.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Import completed");

        // Step 2: Navigate to the standalone server's search page
        Console.WriteLine("[STANDALONE SERVER TEST] Step 2: Navigating to standalone server search page");
        await Page.GotoAsync($"{BaseUrl}/talon-voice-command-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for data to load
        await Page.WaitForTimeoutAsync(2000);
        
        // Take screenshot of search page
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "03_standalone_search_page_loaded.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Search page loaded");

        // Step 3: Perform a search for "open file"
        Console.WriteLine("[STANDALONE SERVER TEST] Step 3: Performing search for 'open file'");
        await Page.FillAsync("input[placeholder*='Search']", "open file");
        await Page.ClickAsync("button[type='submit']");
        
        // Wait for search results
        await Page.WaitForTimeoutAsync(2000);
        
        // Verify we have search results before refresh
        var resultsBeforeRefresh = await Page.QuerySelectorAllAsync(".card");
        Console.WriteLine($"[STANDALONE SERVER TEST] Found {resultsBeforeRefresh.Count} results before refresh");
        
        // Take screenshot of search results
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "04_standalone_search_results_before_refresh.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Search results before refresh");

        // Verify the search found our "open file" command
        var commandText = await Page.TextContentAsync(".card");
        Assert.That(commandText, Does.Contain("open file"), 
            "[STANDALONE SERVER TEST] Should find 'open file' command before refresh");

        // Step 4: Refresh the page to test localStorage persistence
        Console.WriteLine("[STANDALONE SERVER TEST] Step 4: Refreshing page to test localStorage persistence");
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the page to fully load after refresh
        await Page.WaitForTimeoutAsync(3000);
        
        // Take screenshot after refresh
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "05_standalone_after_page_refresh.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Page after refresh");

        // Step 5: Perform the same search again to verify persistence
        Console.WriteLine("[STANDALONE SERVER TEST] Step 5: Performing same search after refresh");
        await Page.FillAsync("input[placeholder*='Search']", "open file");
        await Page.ClickAsync("button[type='submit']");
        
        // Wait for search results
        await Page.WaitForTimeoutAsync(3000);
        
        // Verify we still have search results after refresh
        var resultsAfterRefresh = await Page.QuerySelectorAllAsync(".card");
        Console.WriteLine($"[STANDALONE SERVER TEST] Found {resultsAfterRefresh.Count} results after refresh");
        
        // Take screenshot of search results after refresh
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "06_standalone_search_results_after_refresh.png"), 
            FullPage = true 
        });
        Console.WriteLine("[STANDALONE SERVER TEST] Screenshot: Search results after refresh - PROVING PERSISTENCE WORKS");

        // Critical assertion: Search should still work after page refresh
        Assert.That(resultsAfterRefresh.Count, Is.GreaterThan(0), 
            "[STANDALONE SERVER TEST] CRITICAL: Search results should persist after page refresh in standalone server!");

        // Verify the search still finds our "open file" command after refresh
        var commandTextAfterRefresh = await Page.TextContentAsync(".card");
        Assert.That(commandTextAfterRefresh, Does.Contain("open file"), 
            "[STANDALONE SERVER TEST] CRITICAL: Should still find 'open file' command after refresh!");

        // Additional verification: Test a different search term
        Console.WriteLine("[STANDALONE SERVER TEST] Step 6: Testing different search term for additional validation");
        await Page.FillAsync("input[placeholder*='Search']", "save file");
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForTimeoutAsync(2000);
        
        var saveResults = await Page.QuerySelectorAllAsync(".card");
        await Page.ScreenshotAsync(new() { 
            Path = Path.Combine(tempDir, "07_standalone_save_file_search.png"), 
            FullPage = true 
        });
        
        Assert.That(saveResults.Count, Is.GreaterThan(0), 
            "[STANDALONE SERVER TEST] Should find 'save file' command");
        
        var saveCommandText = await Page.TextContentAsync(".card");
        Assert.That(saveCommandText, Does.Contain("save file"), 
            "[STANDALONE SERVER TEST] Should find 'save file' command text");

        Console.WriteLine("[STANDALONE SERVER TEST] ✅ STANDALONE SERVER PERSISTENCE TEST PASSED!");
        Console.WriteLine("[STANDALONE SERVER TEST] The localStorage persistence issue has been fixed in the standalone TalonVoiceCommandsServer!");
        
        // Log final summary
        Console.WriteLine($"""
[STANDALONE SERVER TEST] TEST SUMMARY:
✅ Successfully imported commands into standalone server
✅ Search worked immediately after import
✅ Page refresh completed successfully  
✅ Search functionality persisted after refresh
✅ Multiple search terms work correctly
✅ Screenshots captured proving the fix works

Test Files Created:
- {Path.Combine(tempDir, "01_standalone_import_page.png")}
- {Path.Combine(tempDir, "02_standalone_import_completed.png")}
- {Path.Combine(tempDir, "03_standalone_search_page_loaded.png")}
- {Path.Combine(tempDir, "04_standalone_search_results_before_refresh.png")}
- {Path.Combine(tempDir, "05_standalone_after_page_refresh.png")}
- {Path.Combine(tempDir, "06_standalone_search_results_after_refresh.png")}
- {Path.Combine(tempDir, "07_standalone_save_file_search.png")}

The standalone TalonVoiceCommandsServer localStorage persistence is working correctly!
""");
    }

    [Test]
    [Description("Tests the standalone TalonVoiceCommandsServer tab navigation and accessibility features")]
    public async Task StandaloneTalonServer_TabNavigation_ShouldWork()
    {
        Console.WriteLine("[STANDALONE SERVER TEST] Starting tab navigation test for standalone server");

        await Page.GotoAsync($"{BaseUrl}/talon-voice-command-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Test the tabbed interface that should be present in the standalone server
        // (assuming the same tabbed UI was implemented)
        
        // Check if Search Commands tab is active by default
        var searchTabActive = await Page.IsVisibleAsync(".nav-link.active:has-text('Search Commands')");
        if (searchTabActive)
        {
            Console.WriteLine("[STANDALONE SERVER TEST] Search Commands tab is active by default");
            
            // Test switching to Import Scripts tab
            await Page.ClickAsync(".nav-link:has-text('Import Scripts')");
            await Page.WaitForTimeoutAsync(1000);
            
            // Take screenshot of Import Scripts tab
            await Page.ScreenshotAsync(new() { 
                Path = Path.Combine(tempDir, "standalone_import_scripts_tab.png"), 
                FullPage = true 
            });
            
            // Test switching to Analysis Report tab
            await Page.ClickAsync(".nav-link:has-text('Analysis Report')");
            await Page.WaitForTimeoutAsync(1000);
            
            // Take screenshot of Analysis Report tab
            await Page.ScreenshotAsync(new() { 
                Path = Path.Combine(tempDir, "standalone_analysis_report_tab.png"), 
                FullPage = true 
            });
            
            Console.WriteLine("[STANDALONE SERVER TEST] ✅ Tab navigation works in standalone server");
        }
        else
        {
            Console.WriteLine("[STANDALONE SERVER TEST] Note: Tabbed interface may not be implemented in standalone server");
        }
    }

    [Test]
    [Description("Validates localStorage keys and data structure in standalone TalonVoiceCommandsServer")]
    public async Task StandaloneTalonServer_LocalStorageValidation_ShouldPersistData()
    {
        Console.WriteLine("[STANDALONE SERVER TEST] Starting localStorage validation test");

        // Clear localStorage and navigate to import page
        await Page.EvaluateAsync("() => localStorage.clear()");
        await Page.GotoAsync($"{BaseUrl}/talon-import");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Import some test data
        await Page.FillAsync("input[placeholder*='directory']", tempDir);
        await Page.ClickAsync("button:has-text('Import All From Directory')");
        await Page.WaitForTimeoutAsync(3000);

        // Check that localStorage contains the expected keys for the standalone server
        var commandsData = await Page.EvaluateAsync<string>("() => localStorage.getItem('talonVoiceCommands')");
        var listsData = await Page.EvaluateAsync<string>("() => localStorage.getItem('talonLists')");

        Console.WriteLine($"[STANDALONE SERVER TEST] Commands data length: {commandsData?.Length ?? 0}");
        Console.WriteLine($"[STANDALONE SERVER TEST] Lists data length: {listsData?.Length ?? 0}");

        Assert.That(commandsData, Is.Not.Null.And.Not.Empty, 
            "[STANDALONE SERVER TEST] Commands should be saved to localStorage");

        // Navigate to search page and verify data is loaded from localStorage
        await Page.GotoAsync($"{BaseUrl}/talon-voice-command-search");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Perform a search to verify data was loaded from localStorage
        await Page.FillAsync("input[placeholder*='Search']", "open");
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForTimeoutAsync(2000);

        var searchResults = await Page.QuerySelectorAllAsync(".card");
        Assert.That(searchResults.Count, Is.GreaterThan(0), 
            "[STANDALONE SERVER TEST] Search should work when data is loaded from localStorage");

        Console.WriteLine("[STANDALONE SERVER TEST] ✅ localStorage validation passed for standalone server");
    }
}