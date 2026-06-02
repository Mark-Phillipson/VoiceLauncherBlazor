using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;

namespace RazorClassLibrary.Pages;

public partial class LauncherAddEdit : ComponentBase
{
    [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
    [CascadingParameter] public IModalService? Modal { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public ICategoryDataService? CategoryDataService { get; set; }
    private List<CategoryDTO> _categories = new List<CategoryDTO>();
    [Inject] public IJSRuntime? JSRuntime { get; set; }
    [Parameter] public int? Id { get; set; }
    [Parameter] public int CategoryID { get; set; }
    public LauncherDTO LauncherDTO { get; set; } = new LauncherDTO();//{ };
    [Inject] public ILauncherDataService? LauncherDataService { get; set; }
    [Inject] public IToastService? ToastService { get; set; }
    [Inject]
    public required ILauncherRepository LauncherRepository { get; set; }
    private HttpClient CreateImageClient()
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        return client;
    }
#pragma warning disable 414, 649
    string TaskRunning = "";
#pragma warning restore 414, 649
    string[] filenameList = new string[0];
    List<ImageItem> imageItems = new List<ImageItem>();
    bool GeneratingImage { get; set; } = false;
    // Property for filter textbox
    public string ImageFilterText { get; set; } = string.Empty;
    // Computed property for filtered image list (exposes paired thumbnail + full image)
    public IEnumerable<ImageItem> FilteredImages =>
        string.IsNullOrWhiteSpace(ImageFilterText)
            ? imageItems
            : imageItems.Where(img => img.Name.Contains(ImageFilterText, StringComparison.OrdinalIgnoreCase) || img.Full.Contains(ImageFilterText, StringComparison.OrdinalIgnoreCase));
    // New property to track icon input mode
    public bool UseCustomIconUrl { get; set; } = false;
    private void SetIconInputMode(bool useCustom)
    {
        UseCustomIconUrl = useCustom;
        if (!useCustom && imageItems.Count > 0 && !imageItems.Any(i => string.Equals(i.Full, LauncherDTO.Icon, StringComparison.OrdinalIgnoreCase) || string.Equals(i.Thumb, LauncherDTO.Icon, StringComparison.OrdinalIgnoreCase)))
        {
            LauncherDTO.Icon = imageItems.First().Full;
        }
    }
    private async Task LoadImages()
    {
        var allowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg", ".ico"
        };
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        Console.WriteLine($"[LauncherAddEdit] Images directory path: {directoryPath}");
        Console.WriteLine($"[LauncherAddEdit] Images directory path: {directoryPath}");
        if (Directory.Exists(directoryPath))
        {
            // Order files by last write time (newest first) so recently-generated thumbnails appear at the top
            var files = Directory.GetFiles(directoryPath)
                .Select(f => new FileInfo(f))
                .Where(fi => allowedImageExtensions.Contains(fi.Extension))
                .OrderByDescending(fi => fi.LastWriteTimeUtc)
                .ToList();

            filenameList = files.Select(fi => fi.FullName).ToArray();
            Console.WriteLine($"[LauncherAddEdit] Found {filenameList.Length} files in images directory (ordered newest-first).");

            // Group files into (full, thumb) pairs by base name (strip "-thumb" suffix)
            var grouped = files.GroupBy(fi => Path.GetFileNameWithoutExtension(fi.Name).Replace("-thumb", string.Empty))
                .Select(g =>
                {
                    var full = g.FirstOrDefault(fi => !Path.GetFileNameWithoutExtension(fi.Name).EndsWith("-thumb"));
                    var thumb = g.FirstOrDefault(fi => Path.GetFileNameWithoutExtension(fi.Name).EndsWith("-thumb"));
                    if (full == null && thumb != null)
                    {
                        // Only a thumb (use it as full)
                        full = thumb;
                    }
                    if (thumb == null && full != null)
                    {
                        // No explicit thumb; use the full image as its own thumb (will be resized client-side)
                        thumb = full;
                    }
                    return new { Full = full, Thumb = thumb };
                })
                .Where(x => x.Full != null)
                .OrderByDescending(x => x.Full!.LastWriteTimeUtc)
                .ToList();

            // Determine which images are referenced by any launcher
            var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (LauncherDataService != null)
            {
                try
                {
                    var allLaunchers = await LauncherDataService.GetAllLaunchersAsync(0);
                    foreach (var l in allLaunchers)
                    {
                        if (!string.IsNullOrWhiteSpace(l.Icon))
                        {
                            referenced.Add(Path.GetFileName(l.Icon) ?? string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LauncherAddEdit] Could not enumerate launchers for image reference check: {ex.Message}");
                }
            }

            imageItems = grouped.Select(x => new ImageItem
            {
                Full = x.Full!.Name,
                Thumb = x.Thumb!.Name,
                IsLinked = referenced.Contains(x.Full!.Name)
            }).ToList();
        }
        else
        {
            Console.WriteLine($"[LauncherAddEdit] Images directory does not exist.");
            imageItems = new List<ImageItem>();
        }
    }

    public class ImageItem
    {
        public string Full { get; set; } = string.Empty;
        public string Thumb { get; set; } = string.Empty;
        public string Name => Path.GetFileName(Full) ?? Path.GetFileName(Thumb) ?? string.Empty;
        public bool IsLinked { get; set; } = false;
    }

    // Preview modal state
    bool IsImagePreviewOpen { get; set; } = false;
    string PreviewImageFilename { get; set; } = string.Empty;
    // Last-generation debug fields
    public string LastImagePrompt { get; set; } = string.Empty;
    public string LastImageProviderResponse { get; set; } = string.Empty;
    public string LastImageModel { get; set; } = string.Empty;
    // Prompt builder / manual upload UI state
    public bool UseAiGeneration { get; set; } = false;
    public bool PromptBuilderVisible { get; set; } = false;
    public string PromptBuilderText { get; set; } = string.Empty;

    protected void OpenImagePreview(string filename)
    {
        PreviewImageFilename = filename ?? string.Empty;
        IsImagePreviewOpen = true;
        StateHasChanged();
    }

    protected void CloseImagePreview()
    {
        IsImagePreviewOpen = false;
        PreviewImageFilename = string.Empty;
        StateHasChanged();
    }

    protected void SelectPreviewImage()
    {
        if (!string.IsNullOrEmpty(PreviewImageFilename))
        {
            LauncherDTO.Icon = PreviewImageFilename;
            CloseImagePreview();
        }
    }

    protected void SelectImage(string filename)
    {
        LauncherDTO.Icon = filename;
        StateHasChanged();
    }

    protected async Task CopyPromptToClipboard()
    {
        try
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", PromptBuilderText ?? string.Empty);
                ToastService?.ShowSuccess("Prompt copied to clipboard");
            }
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Copy failed: {ex.Message}");
        }
    }

    protected void OpenThirdPartySite()
    {
        try
        {
            JSRuntime?.InvokeVoidAsync("open", "https://www.bing.com/images/create", "_blank");
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Unable to open site: {ex.Message}");
        }
    }

    protected async Task UsePromptToGenerateAsync()
    {
        if (string.IsNullOrWhiteSpace(PromptBuilderText))
        {
            ToastService?.ShowError("Prompt is empty");
            return;
        }

        try
        {
            using var http = CreateImageClient();
            var baseUri = new Uri(NavigationManager.BaseUri);
            var apiUri = new Uri(baseUri, "api/images/generate");
            var response = await http.PostAsJsonAsync(apiUri, new { prompt = PromptBuilderText });
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                ToastService?.ShowError($"Image generation failed: {response.ReasonPhrase} - {body}");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<ImageGenerationResult>();
            if (result != null)
            {
                LauncherDTO.Icon = result.Filename;
                LastImagePrompt = result.Prompt ?? string.Empty;
                LastImageProviderResponse = result.ProviderResponse ?? string.Empty;
                LastImageModel = result.Model ?? string.Empty;
                PromptBuilderVisible = false;
                await LoadImages();
                ToastService?.ShowSuccess("Generated image is ready - select it or save the launcher.");
            }
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Error generating image: {ex.Message}");
        }
    }

    protected async Task UploadImageAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null)
        {
            ToastService?.ShowError("No file selected");
            return;
        }

        try
        {
            using var http = CreateImageClient();
            var baseUri = new Uri(NavigationManager.BaseUri);
            var apiUri = new Uri(baseUri, "api/images/upload");
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(10 * 1024 * 1024);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.Name);
            var response = await http.PostAsync(apiUri, content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                ToastService?.ShowError($"Upload failed: {response.ReasonPhrase} - {body}");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<ImageGenerationResult>();
            if (result != null)
            {
                LauncherDTO.Icon = result.Filename;
                LastImagePrompt = result.Prompt ?? string.Empty;
                LastImageProviderResponse = result.ProviderResponse ?? string.Empty;
                LastImageModel = result.Model ?? string.Empty;
            }

            await LoadImages();
            ToastService?.ShowSuccess("Image uploaded");
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Error uploading image: {ex.Message}");
        }
    }

    protected async Task DeleteImageAsync(string filename)
    {
        try
        {
            using var http = CreateImageClient();
            var baseUri = new Uri(NavigationManager.BaseUri);
            var apiUri = new Uri(baseUri, "api/images/delete");
            var payload = new { filename = filename, force = false };
            var response = await http.PostAsJsonAsync(apiUri, payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                ToastService?.ShowError($"Delete failed: {response.ReasonPhrase} - {body}");
                return;
            }

            var bodyText = await response.Content.ReadAsStringAsync();
            ToastService?.ShowSuccess("Image deleted");
            await LoadImages();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Error deleting image: {ex.Message}");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (LauncherDataService == null)
        {
            return;
        }
        if (Id > 0)
        {
            var result = await LauncherDataService.GetLauncherById((int)Id);
            if (result != null)
            {
                LauncherDTO = result;
            }
        }
        else
        {
            LauncherDTO.CategoryId = CategoryID;
        }
        if (CategoryDataService != null)
        {
            _categories = await CategoryDataService.GetAllCategoriesAsync("Launch Applications", 0);
        }
        await LoadImages();

        if (LauncherDTO.Id > 0)
        {
            // Load existing category associations
            var categoryIds = await LauncherRepository!.GetCategoryIdsForLauncherAsync(LauncherDTO.Id);
            SelectedCategoryIds = new HashSet<int>(categoryIds);

            // Keep the primary category if it exists
            if (LauncherDTO.CategoryId > 0 && !SelectedCategoryIds.Contains(LauncherDTO.CategoryId))
            {
                SelectedCategoryIds.Add(LauncherDTO.CategoryId);
            }
        }
        else if (LauncherDTO.CategoryId > 0)
        {
            // For new launcher with default category
            SelectedCategoryIds.Add(LauncherDTO.CategoryId);
        }
    }

    private class ImageGenerationResult
    {
        public string Filename { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string ProviderResponse { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    protected async Task GenerateImageAsync()
    {
        // If AI generation is disabled, open the prompt builder instead of calling the provider.
        if (!UseAiGeneration)
        {
            PromptBuilderText = $"Create a simple launcher thumbnail for: {LauncherDTO.Name}. Context: {LauncherDTO.CommandLine}.";
            PromptBuilderVisible = true;
            StateHasChanged();
            return;
        }

        GeneratingImage = true;
        StateHasChanged();

        try
        {
            using var http = CreateImageClient();
            ImageGenerationResult? result = null;
            var baseUri = new Uri(NavigationManager.BaseUri);
            if (LauncherDTO.Id > 0)
            {
                var apiUri = new Uri(baseUri, $"api/launchers/{LauncherDTO.Id}/generate-image");
                var response = await http.PostAsync(apiUri, null);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    ToastService?.ShowError($"Image generation failed: {response.ReasonPhrase} - {body}");
                    return;
                }
                result = await response.Content.ReadFromJsonAsync<ImageGenerationResult>();
            }
            else
            {
                var apiUri = new Uri(baseUri, "api/launchers/generate-image");
                var response = await http.PostAsJsonAsync(apiUri, LauncherDTO);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    ToastService?.ShowError($"Image generation failed: {response.ReasonPhrase} - {body}");
                    return;
                }
                result = await response.Content.ReadFromJsonAsync<ImageGenerationResult>();
            }

            if (result != null)
            {
                LauncherDTO.Icon = result.Filename;
                LastImagePrompt = result.Prompt ?? string.Empty;
                LastImageProviderResponse = result.ProviderResponse ?? string.Empty;
                LastImageModel = result.Model ?? string.Empty;
                await LoadImages();
                ToastService?.ShowSuccess("Generated image is ready - select it or save the launcher.");
                Console.WriteLine($"[ImageGen] Prompt: {LastImagePrompt}");
                Console.WriteLine($"[ImageGen] ProviderResponse: {LastImageProviderResponse}");
            }
        }
        catch (Exception ex)
        {
            ToastService?.ShowError($"Error generating image: {ex.Message}");
        }
        finally
        {
            GeneratingImage = false;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("setFocus", "Name");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
    public void Close()
    {
        if (ModalInstance != null)
            ModalInstance.CancelAsync();
    }

    protected void CategoryCheckboxChanged(int categoryId, ChangeEventArgs e)
    {
        bool isChecked = false;
        if (e?.Value is bool b)
            isChecked = b;
        else if (e?.Value?.ToString() == "true")
            isChecked = true;

        if (isChecked)
        {
            SelectedCategoryIds.Add(categoryId);
            if (LauncherDTO.CategoryId <= 0)
            {
                LauncherDTO.CategoryId = categoryId;
            }
        }
        else
        {
            SelectedCategoryIds.Remove(categoryId);
            if (LauncherDTO.CategoryId == categoryId && SelectedCategoryIds.Any())
            {
                LauncherDTO.CategoryId = SelectedCategoryIds.First();
            }
        }
    }

    protected async Task HandleValidSubmit()
    {
        // Validate that at least one category is selected
        if (!SelectedCategoryIds.Any() && LauncherDTO.CategoryId <= 0)
        {
            ToastService?.ShowError("Please select at least one category.");
            return;
        }

        // Make sure primary category is in selected categories
        if (LauncherDTO.CategoryId <= 0 && SelectedCategoryIds.Any())
        {
            LauncherDTO.CategoryId = SelectedCategoryIds.First();
        }

        // Save launcher first
        LauncherDTO? savedLauncher;
        if (LauncherDTO.Id > 0)
        {
            if (LauncherRepository != null)
            {
                savedLauncher = await LauncherRepository.UpdateLauncherAsync(LauncherDTO);
            }
            else
            {
                ToastService?.ShowError("LauncherRepository is not initialized.");
                return;
            }
        }
        else
        {
            savedLauncher = await LauncherRepository!.AddLauncherAsync(LauncherDTO);
        }

        if (savedLauncher != null)
        {
            // Update category associations
            await LauncherRepository.UpdateLauncherCategoriesAsync(savedLauncher.Id, SelectedCategoryIds);

            ToastService?.ShowSuccess("Launcher saved successfully");
        }
        else
        {
            ToastService?.ShowError("Launcher failed to save, please investigate Error Adding New Launcher");
        }

        if (ModalInstance != null)
        {
            await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = "";
        }

        // Add this property to store selected category IDs
        protected HashSet<int> SelectedCategoryIds { get; set; } = new HashSet<int>();
        private void GoBack()
        {
            NavigationManager.NavigateTo($"/launcherstable/{LauncherDTO.CategoryId}");
        }

        protected void ToggleFavourite()
        {
            LauncherDTO.Favourite = !LauncherDTO.Favourite;
            StateHasChanged();
        }
    }
