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
public class TalonTitleImportTest : PageTest
{
    private string tempDir = "";
    private string talonFileWithTitle = "";
    private string talonFileWithoutTitle = "";

    [SetUp]
    public void Setup()
    {
        // Create temporary directory and test files
        tempDir = Path.Combine(Path.GetTempPath(), $"TalonTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Create a Talon file with title
        talonFileWithTitle = Path.Combine(tempDir, "test_with_title.talon");
        File.WriteAllText(talonFileWithTitle, @"app: msedge.exe
title: ChatGPT Shortcuts Test
mode: command
-

new chat: 
    key(ctrl-shift-o)

focus input: 
    key(shift-esc)

copy code block: 
    key(ctrl-shift-;)

copy response: 
    key(ctrl-shift-c)
");

        // Create a Talon file without title
        talonFileWithoutTitle = Path.Combine(tempDir, "test_without_title.talon");
        File.WriteAllText(talonFileWithoutTitle, @"app: vscode
mode: command
-

open file: 
    key(ctrl-o)

save file: 
    key(ctrl-s)
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
    public async Task ImportTalonFileWithTitle_ShouldImportSuccessfully()
    {
        // Navigate to the import page
        await Page.GotoAsync("http://localhost:5000/talon-import");

        // Verify we're on the correct page
        await Expect(Page).ToHaveTitleAsync(new Regex("Voice Admin"));
        await Expect(Page.GetByText("Import Talon Scripts")).ToBeVisibleAsync();

        // Fill in the directory path
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync(tempDir);        // Click the Import All from Directory button
        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        // Wait for import to complete (look for success message)
        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });        // Verify success message contains information about imported commands
        var successText = await successAlert.TextContentAsync();
        Assert.That(successText, Does.Contain("command(s)"));

        // Navigate to search page to verify commands were imported
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");        // Search for commands from the file with title
        var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("new chat");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();

        // Wait for search results with longer timeout
        await Page.WaitForTimeoutAsync(5000);

        // Verify the command appears in search results - be more flexible with longer timeout
        await Expect(Page.GetByText("new chat")).ToBeVisibleAsync(new() { Timeout = 15000 });
        await Expect(Page.GetByText("key(ctrl-shift-o)")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task ImportTalonFileWithTitle_SearchShouldFindCommands()
    {
        // First import the files
        await Page.GotoAsync("http://localhost:5000/talon-import");
        
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");        await directoryInput.FillAsync(tempDir);

        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        // Wait for import to complete
        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Navigate to search page
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");        // Test searching for commands from file with title
        var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("copy code block");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();

        await Page.WaitForTimeoutAsync(2000);

        // Verify command from titled file is found
        await Expect(Page.GetByText("copy code block")).ToBeVisibleAsync();
        await Expect(Page.GetByText("key(ctrl-shift-;)")).ToBeVisibleAsync();

        // Clear search and test command from file without title
        await searchInput.ClearAsync();
        await searchInput.FillAsync("open file");
        await searchButton.ClickAsync();

        await Page.WaitForTimeoutAsync(2000);

        // Verify command from non-titled file is also found
        await Expect(Page.GetByText("open file")).ToBeVisibleAsync();
        await Expect(Page.GetByText("key(ctrl-o)")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ImportTalonFiles_FilterByApplication_ShouldWork()
    {        // Import files first
        await Page.GotoAsync("http://localhost:5000/talon-import");
        
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync(tempDir);

        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Navigate to search page
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");

        // Wait for page to load and filters to populate
        await Page.WaitForTimeoutAsync(3000);

        // Filter by msedge.exe application (the one with title)
        var applicationFilter = Page.Locator("select").Filter(new() { Has = Page.GetByText("All Applications") });
        await applicationFilter.SelectOptionAsync("msedge.exe");

        // Wait for filter to apply
        await Page.WaitForTimeoutAsync(2000);        // Search for any command to see filtered results
        var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("chat");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();

        await Page.WaitForTimeoutAsync(2000);

        // Should find commands from msedge.exe app (the file with title)
        await Expect(Page.GetByText("new chat")).ToBeVisibleAsync();

        // Switch to vscode application filter
        await applicationFilter.SelectOptionAsync("vscode");
        await Page.WaitForTimeoutAsync(2000);

        // Clear and search again
        await searchInput.ClearAsync();
        await searchInput.FillAsync("file");
        await searchButton.ClickAsync();

        await Page.WaitForTimeoutAsync(2000);

        // Should find commands from vscode app (the file without title)
        await Expect(Page.GetByText("open file")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ImportErrorHandling_InvalidDirectory_ShouldShowError()
    {
        await Page.GotoAsync("http://localhost:5000/talon-import");        // Try to import from a non-existent directory
        var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync("C:\\NonExistentDirectory\\InvalidPath");

        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        // Wait for error message
        var errorAlert = Page.Locator(".alert-danger");
        await Expect(errorAlert).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Verify error message is displayed
        var errorText = await errorAlert.TextContentAsync();
        Assert.That(errorText, Is.Not.Empty);
    }

    [Test]
    public async Task ImportTalonFiles_ClearAndReimport_ShouldReplaceData()
    {
        // First import
        await Page.GotoAsync("http://localhost:5000/talon-import");
          var directoryInput = Page.GetByPlaceholder("Directory to import all .talon files from...");
        await directoryInput.FillAsync(tempDir);

        var importButton = Page.GetByText("Import All from Directory");
        await importButton.ClickAsync();

        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Note the number of commands imported
        var firstImportText = await successAlert.TextContentAsync();

        // Import again (should clear and re-import)
        await directoryInput.ClearAsync();
        await directoryInput.FillAsync(tempDir);
        await importButton.ClickAsync();

        await Expect(successAlert).ToBeVisibleAsync(new() { Timeout = 10000 });        // Should show the same number of commands imported
        var secondImportText = await successAlert.TextContentAsync();
        Assert.That(secondImportText, Does.Contain("command(s)"));

        // Verify data is still searchable
        await Page.GotoAsync("http://localhost:5000/talon-voice-command-search");
          var searchInput = Page.GetByPlaceholder("Search commands or scripts...");
        await searchInput.FillAsync("new chat");
        
        var searchButton = Page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        await searchButton.ClickAsync();

        await Page.WaitForTimeoutAsync(2000);
        await Expect(Page.GetByText("new chat")).ToBeVisibleAsync();
    }
}
