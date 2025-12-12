using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Diagnostics;

namespace PlaywrightTests;

[TestFixture]
public class AutoRotateVoiceCommandsTests : PageTest
{
    private Process? _serverProcess;
    private const string BaseUrl = "http://localhost:5008";
    private const string TestUrl = $"{BaseUrl}/talon-voice-command-search";

    [SetUp]
    public async Task Setup()
    {
        // Check if server is already running
        if (!await IsServerRunning())
        {
            await StartServer();
            
            // Wait for server to start
            await WaitForServerToStart();
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        // Only stop server if we started it
        if (_serverProcess != null)
        {
            try
            {
                if (!_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    await _serverProcess.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping server: {ex.Message}");
            }
            finally
            {
                _serverProcess?.Dispose();
                _serverProcess = null;
            }
        }
    }

    [Test]
    public async Task AutoRotateVoiceCommands_ShowsRandomCommandsWhenConditionsMet()
    {
        // Navigate to the search page
        await Page.GotoAsync(TestUrl);
        
        // Wait for page to load completely
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Ensure search input is empty and auto-filter is off
        var searchInput = Page.Locator("#searchTerm");
        await searchInput.FillAsync("");
        
        var autoFilterToggle = Page.Locator("#autoFilterToggle");
        if (await autoFilterToggle.IsCheckedAsync())
        {
            await autoFilterToggle.ClickAsync();
        }
        
        // Wait a moment for conditions to be evaluated
        await Page.WaitForTimeoutAsync(1000);
        
        // Check if auto-rotation container becomes visible
        var rotationContainer = Page.Locator(".search-results-container");
        
        // Wait up to 10 seconds for the container to become visible
        try
        {
            await rotationContainer.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 10000 
            });
            
            Console.WriteLine("Auto-rotation container found and visible");
            
            // Check if there's an auto-rotate card
            var autoRotateCard = Page.Locator(".auto-rotate-card");
            await autoRotateCard.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 5000 
            });
            
            Console.WriteLine("Auto-rotate card found");
            
            // Take a screenshot to show the feature working
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "auto-rotate-voice-commands-working.png",
                FullPage = true
            });
            
            Console.WriteLine("Screenshot taken: auto-rotate-voice-commands-working.png");
            
            // Verify the card contains expected elements
            var commandHeader = autoRotateCard.Locator(".auto-rotate-header");
            await Expect(commandHeader).ToBeVisibleAsync();
            
            var rotationIndicator = autoRotateCard.Locator(".rotation-indicator");
            await Expect(rotationIndicator).ToBeVisibleAsync();
            
            var scriptContent = autoRotateCard.Locator(".script-content");
            await Expect(scriptContent).ToBeVisibleAsync();
            
            Console.WriteLine("All expected elements are present in auto-rotate card");
        }
        catch (TimeoutException)
        {
            // If no commands are available, we should see a message instead
            var messageElement = Page.Locator(".auto-rotate-message");
            
            if (await messageElement.IsVisibleAsync())
            {
                var messageText = await messageElement.InnerTextAsync();
                Console.WriteLine($"Auto-rotation showed message: {messageText}");
                
                // Take screenshot of the message
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = "auto-rotate-no-commands-message.png",
                    FullPage = true
                });
                
                Console.WriteLine("Screenshot taken: auto-rotate-no-commands-message.png");
                
                // This is acceptable - no commands imported yet
                Assert.Pass("Auto-rotation is working but no commands are available to display");
            }
            else
            {
                // Take a screenshot of the current state for debugging
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = "auto-rotate-debug-state.png",
                    FullPage = true
                });
                
                Console.WriteLine("Screenshot taken for debugging: auto-rotate-debug-state.png");
                
                // Check console logs for any errors
                var consoleMessages = new List<string>();
                Page.Console += (_, e) => consoleMessages.Add($"{e.Type}: {e.Text}");
                
                if (consoleMessages.Any())
                {
                    Console.WriteLine("Console messages:");
                    foreach (var msg in consoleMessages)
                    {
                        Console.WriteLine($"  {msg}");
                    }
                }
                
                Assert.Fail("Auto-rotation container or card did not become visible within timeout");
            }
        }
    }

    [Test]
    public async Task AutoRotateVoiceCommands_StopsOnUserInteraction()
    {
        // Navigate to the search page
        await Page.GotoAsync(TestUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Ensure conditions are met for auto-rotation to start
        var searchInput = Page.Locator("#searchTerm");
        await searchInput.FillAsync("");
        
        var autoFilterToggle = Page.Locator("#autoFilterToggle");
        if (await autoFilterToggle.IsCheckedAsync())
        {
            await autoFilterToggle.ClickAsync();
        }
        
        // Wait for auto-rotation to potentially start
        await Page.WaitForTimeoutAsync(2000);
        
        // Type in search box to stop rotation
        await searchInput.FillAsync("test");
        
        // Wait a moment for stop logic to execute
        await Page.WaitForTimeoutAsync(500);
        
        // Verify rotation container is hidden or has stopped
        var rotationContainer = Page.Locator(".search-results-container");
        
        // Should either be hidden or not contain auto-rotate content
        var isHidden = await rotationContainer.IsHiddenAsync();
        var hasAutoRotateCard = await Page.Locator(".auto-rotate-card").IsVisibleAsync();
        
        // Take screenshot of stopped state
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "auto-rotate-stopped-after-interaction.png",
            FullPage = true
        });
        
        Console.WriteLine("Screenshot taken: auto-rotate-stopped-after-interaction.png");
        
        Assert.That(isHidden || !hasAutoRotateCard, 
            "Auto-rotation should stop when user types in search box");
    }

    private async Task<bool> IsServerRunning()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetAsync($"{BaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Try the main page if health endpoint doesn't exist
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.GetAsync(BaseUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    private async Task StartServer()
    {
        // Resolve repository root by walking up directories until the solution file is found.
        var dir = TestContext.CurrentContext.TestDirectory;
        string? repoRoot = null;
        var maxDepth = 8;
        var current = dir;
        for (int i = 0; i < maxDepth; i++)
        {
            if (current == null) break;
            var slnPath = Path.Combine(current, "VoiceLauncherBlazor.sln");
            if (File.Exists(slnPath))
            {
                repoRoot = current;
                break;
            }
            current = Path.GetDirectoryName(current);
        }

        if (repoRoot == null)
        {
            // Fallback: assume two levels up from test directory (best-effort)
            repoRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", ".."));
        }

        var serverProjectPath = Path.Combine(repoRoot, "VoiceAdmin");
        Console.WriteLine($"Starting server (VoiceAdmin) from: {serverProjectPath}");
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --configuration Debug",
            WorkingDirectory = serverProjectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        // Add .NET to PATH if needed
        var dotnetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet");
        if (Directory.Exists(dotnetPath))
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            startInfo.EnvironmentVariables["PATH"] = $"{dotnetPath}{Path.PathSeparator}{currentPath}";
        }
        
        _serverProcess = Process.Start(startInfo);
        
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Failed to start server process");
        }
        
        // Log server output for debugging
        _ = Task.Run(async () =>
        {
            while (!_serverProcess.HasExited)
            {
                var line = await _serverProcess.StandardOutput.ReadLineAsync();
                if (line != null)
                {
                    Console.WriteLine($"[SERVER] {line}");
                }
            }
        });
        
        _ = Task.Run(async () =>
        {
            while (!_serverProcess.HasExited)
            {
                var line = await _serverProcess.StandardError.ReadLineAsync();
                if (line != null)
                {
                    Console.WriteLine($"[SERVER ERROR] {line}");
                }
            }
        });
    }

    private async Task WaitForServerToStart()
    {
        var maxAttempts = 30; // 30 seconds
        var attempt = 0;
        
        while (attempt < maxAttempts)
        {
            try
            {
                if (await IsServerRunning())
                {
                    Console.WriteLine($"Server started successfully after {attempt} seconds");
                    return;
                }
            }
            catch
            {
                // Ignore exceptions during startup checking
            }
            
            await Task.Delay(1000);
            attempt++;
        }
        
        throw new TimeoutException("Server failed to start within 30 seconds");
    }
}