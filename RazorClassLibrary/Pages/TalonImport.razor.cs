using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RCLTalonShared.Services;
using RCLTalonShared.Models;

namespace RazorClassLibrary.Pages
{
    public partial class TalonImport : ComponentBase
    {
        public IReadOnlyList<IBrowserFile>? SelectedFiles { get; set; }
        public bool IsLoading { get; set; } = false;
        public string? ImportResult { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DirectoryPath { get; set; } = @"C:\Users\MPhil\AppData\Roaming\talon\user";
        public string? ListsFilePath { get; set; } = @"C:\Users\MPhil\AppData\Roaming\talon\user\mystuff\talon_my_stuff\TalonLists.txt";
        public int ImportProgress { get; set; } = 0;
        public int ImportTotal { get; set; } = 0;
        private bool isHybridMode = false;

        protected override async Task OnInitializedAsync()
        {
            await DetectHybridModeAsync();
        }

        private async Task DetectHybridModeAsync()
        {
            try
            {
                // Check if we're running in a WebView (Blazor Hybrid)
                isHybridMode = await JSRuntime.InvokeAsync<bool>("eval", 
                    "window.chrome && window.chrome.webview != null");
            }
            catch
            {
                // If detection fails, default to false (server mode)
                isHybridMode = false;
            }
        }

        protected void OnFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFiles = e.GetMultipleFiles();
            ImportResult = null;
            ErrorMessage = null;
        }        protected async Task ImportFiles()
        {
            if (SelectedFiles == null || SelectedFiles.Count == 0)
                return;
            IsLoading = true;
            ImportResult = null;
            ErrorMessage = null;
            int totalCommandsImported = 0;
            try
            {
                foreach (var file in SelectedFiles)
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    // Parse commands using shared import service and save via repository
                    var parsed = TalonImportService.ParseTalonFile(content, file.Name, null);
                    await TalonRepository.SaveCommandsAsync(parsed);
                    totalCommandsImported += parsed.Count;
                }
                ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {SelectedFiles.Count} file(s).";
            }            catch (Exception ex)
            {
                ErrorMessage = $"Error importing files: {GetFullErrorMessage(ex)}";
            }
            finally
            {
                IsLoading = false;
            }
        }        protected async Task ImportAllFromDirectory()
        {
            if (string.IsNullOrWhiteSpace(DirectoryPath))
            {
                ErrorMessage = "Please enter a directory path.";
                return;
            }
            IsLoading = true;
            ImportResult = null;
            ErrorMessage = null;
            ImportProgress = 0;
            ImportTotal = 0;
            try
            {
                var talonFiles = Directory.GetFiles(DirectoryPath, "*.talon", SearchOption.AllDirectories);
                ImportTotal = talonFiles.Length;
                
                // Use the new service method with progress callback
                // For RCL hosted in server mode, we still try to reuse the existing TalonServiceField behavior if available.
                // Fallback: perform a simple directory parse using TalonImportService and TalonRepository.
                var talonFiles = Directory.GetFiles(DirectoryPath, "*.talon", SearchOption.AllDirectories);
                ImportTotal = talonFiles.Length;
                int commandsSoFar = 0;
                int filesProcessed = 0;
                foreach(var f in talonFiles)
                {
                    var content = await File.ReadAllTextAsync(f);
                    var parsed = TalonImportService.ParseTalonFile(content, Path.GetFileName(f), f);
                    await TalonRepository.SaveCommandsAsync(parsed);
                    commandsSoFar += parsed.Count;
                    filesProcessed++;
                    ImportProgress = filesProcessed;
                    StateHasChanged();
                }
                var totalCommandsImported = commandsSoFar;
                
                ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {ImportTotal} .talon files in directory.";
                
                // Invalidate the filter cache so repository dropdown gets updated
                TalonVoiceCommandSearch.InvalidateFilterCache();
            }            catch (Exception ex)
            {
                ErrorMessage = $"Error importing from directory: {GetFullErrorMessage(ex)}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ImportListsFromFile()
        {
            if (string.IsNullOrWhiteSpace(ListsFilePath))
            {
                ErrorMessage = "Please provide a valid path to the TalonLists.txt file.";
                return;
            }

            IsLoading = true;
            ImportResult = null;
            ErrorMessage = null;
            StateHasChanged();
 
            try
            {
                var listsContent = await File.ReadAllTextAsync(ListsFilePath);
                var parsedLists = TalonImportService.ParseTalonListsFile(listsContent, ListsFilePath);
                await TalonRepository.SaveListsAsync(parsedLists);
                ImportResult = $"Successfully imported {parsedLists.Count} list items from {Path.GetFileName(ListsFilePath)}.";
            }            catch (Exception ex)
            {
                ErrorMessage = $"Error importing lists: {GetFullErrorMessage(ex)}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetFullErrorMessage(Exception ex)
        {
            var errorParts = new List<string>();
            var currentEx = ex;
            
            while (currentEx != null)
            {
                errorParts.Add(currentEx.Message);
                currentEx = currentEx.InnerException;
            }
            
            return string.Join(" | Inner Exception: ", errorParts);
        }
    }
}
// Note: IBrowserFile does not provide the full path for security reasons. For browser uploads, only the filename is available. For server-side directory imports, the full path is already used.
