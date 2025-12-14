using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System;
using System.IO;

namespace PlaywrightTests;

[TestClass]
public class FaceRecognitionTests
{
    private const string BaseUrl = "http://localhost:5008";
    private const string FaceRecognitionUrl = $"{BaseUrl}/face-recognition";

    [TestMethod]
    public async Task FaceRecognition_FullWorkflow_Test()
    {
        // Create Playwright
        using var playwright = await Playwright.CreateAsync();

        // Launch browser (set Headless = false to see the browser)
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
        { 
            Headless = false,
            SlowMo = 500 // Slow down operations for better visibility
        });
        
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1600, Height = 900 }
        });
        var page = await context.NewPageAsync();

        Console.WriteLine($"Navigating to {FaceRecognitionUrl}");

        // Navigate and wait for network idle
        await page.GotoAsync(FaceRecognitionUrl, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });

        // Take initial screenshot
        var initialPath = Path.Combine(Environment.CurrentDirectory, "face-recognition-initial.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = initialPath, FullPage = true });
        Console.WriteLine($"Saved initial screenshot: {initialPath}");

        // Wait for the page to be fully loaded
        await page.WaitForSelectorAsync("h2:has-text('Face Recognition & Tagging')", new() { Timeout = 10000 });
        Console.WriteLine("Face Recognition page loaded");

        // Check that upload form is visible
        var imageNameInput = await page.QuerySelectorAsync("#imageName");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(imageNameInput, "Image name input should be visible");
        Console.WriteLine("Upload form is visible");

        // Create a test image file (simple 1x1 pixel base64 PNG)
        var testImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        var testImageBytes = Convert.FromBase64String(testImageBase64);
        var tempImagePath = Path.Combine(Path.GetTempPath(), "test-face-image.png");
        await File.WriteAllBytesAsync(tempImagePath, testImageBytes);
        Console.WriteLine($"Created test image at: {tempImagePath}");

        // Fill in the image name
        await imageNameInput.FillAsync("Test Group Photo");
        Console.WriteLine("Filled image name");

        // Fill in description
        var descriptionInput = await page.QuerySelectorAsync("#imageDescription");
        if (descriptionInput != null)
        {
            await descriptionInput.FillAsync("Test photo for face tagging demo");
            Console.WriteLine("Filled description");
        }

        // Upload the image file
        var fileInput = await page.QuerySelectorAsync("#imageFile");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(fileInput, "File input should be visible");
        await fileInput.SetInputFilesAsync(tempImagePath);
        Console.WriteLine("Selected file for upload");

        // Click upload button
        var uploadButton = await page.QuerySelectorAsync("button:has-text('Upload Image')");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(uploadButton, "Upload button should be visible");
        await uploadButton.ClickAsync();
        Console.WriteLine("Clicked upload button");

        // Wait for success message or image to load
        try
        {
            await page.WaitForSelectorAsync(".alert-success", new() { Timeout = 10000 });
            Console.WriteLine("Upload success message appeared");
        }
        catch
        {
            Console.WriteLine("No success message, checking if image loaded");
        }

        // Take screenshot after upload
        var afterUploadPath = Path.Combine(Environment.CurrentDirectory, "face-recognition-after-upload.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = afterUploadPath, FullPage = true });
        Console.WriteLine($"Saved after-upload screenshot: {afterUploadPath}");

        // Wait a bit for the image to be displayed
        await Task.Delay(1000);

        // Check if the image is displayed in the viewer
        var displayedImage = await page.QuerySelectorAsync("img.img-fluid");
        if (displayedImage != null)
        {
            Console.WriteLine("Image is displayed in the viewer");

            // Try to start tagging
            var addTagButton = await page.QuerySelectorAsync("button:has-text('Add Face Tag')");
            if (addTagButton != null)
            {
                await addTagButton.ClickAsync();
                Console.WriteLine("Clicked 'Add Face Tag' button");
                await Task.Delay(500);

                // Take screenshot in tagging mode
                var taggingModePath = Path.Combine(Environment.CurrentDirectory, "face-recognition-tagging-mode.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = taggingModePath, FullPage = true });
                Console.WriteLine($"Saved tagging-mode screenshot: {taggingModePath}");

                // Click on the image to create a tag
                var imageBounds = await displayedImage.BoundingBoxAsync();
                if (imageBounds != null)
                {
                    // Click in the center of the image
                    await page.Mouse.ClickAsync(
                        imageBounds.X + imageBounds.Width / 2,
                        imageBounds.Y + imageBounds.Height / 2
                    );
                    Console.WriteLine("Clicked on image to create tag");
                    await Task.Delay(500);

                    // Enter a name for the tag
                    var nameInput = await page.QuerySelectorAsync("input[placeholder='Enter first name']");
                    if (nameInput != null)
                    {
                        await nameInput.FillAsync("John");
                        Console.WriteLine("Entered name 'John' for the tag");

                        // Save the tag
                        var saveButton = await page.QuerySelectorAsync("button:has-text('Save')");
                        if (saveButton != null)
                        {
                            await saveButton.ClickAsync();
                            Console.WriteLine("Clicked save button for tag");
                            await Task.Delay(1000);

                            // Take screenshot with tag
                            var withTagPath = Path.Combine(Environment.CurrentDirectory, "face-recognition-with-tag.png");
                            await page.ScreenshotAsync(new PageScreenshotOptions { Path = withTagPath, FullPage = true });
                            Console.WriteLine($"Saved with-tag screenshot: {withTagPath}");
                        }
                    }
                }

                // Test search functionality
                var searchInput = await page.QuerySelectorAsync("#nameSearch");
                if (searchInput != null)
                {
                    await searchInput.FillAsync("John");
                    Console.WriteLine("Entered 'John' in search box");
                    await Task.Delay(500);

                    // Take screenshot with search highlighting
                    var searchHighlightPath = Path.Combine(Environment.CurrentDirectory, "face-recognition-search-highlight.png");
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = searchHighlightPath, FullPage = true });
                    Console.WriteLine($"Saved search-highlight screenshot: {searchHighlightPath}");
                }
            }
        }

        // Take final screenshot
        var finalPath = Path.Combine(Environment.CurrentDirectory, "face-recognition-final.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = finalPath, FullPage = true });
        Console.WriteLine($"Saved final screenshot: {finalPath}");

        // Clean up temp file
        if (File.Exists(tempImagePath))
        {
            File.Delete(tempImagePath);
            Console.WriteLine("Cleaned up temp test image");
        }

        await browser.CloseAsync();
        Console.WriteLine("Test completed successfully");
    }

    [TestMethod]
    public async Task FaceRecognition_PageLoads_Test()
    {
        // Simple test to verify the page loads
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        Console.WriteLine($"Navigating to {FaceRecognitionUrl}");
        await page.GotoAsync(FaceRecognitionUrl, new PageGotoOptions { Timeout = 60000 });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 30000 });

        // Check that the main heading is present
        var heading = await page.QuerySelectorAsync("h2:has-text('Face Recognition & Tagging')");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(heading, "Page heading should be present");

        // Check that upload form elements are present
        var imageName = await page.QuerySelectorAsync("#imageName");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(imageName, "Image name input should be present");

        var imageFile = await page.QuerySelectorAsync("#imageFile");
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(imageFile, "File input should be present");

        Console.WriteLine("Face Recognition page loaded successfully with all required elements");
        await browser.CloseAsync();
    }

    public Microsoft.VisualStudio.TestTools.UnitTesting.TestContext? TestContext { get; set; }
}
