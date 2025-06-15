using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DataAccessLibrary.Services;
using DataAccessLibrary;
using DataAccessLibrary.Models;

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
                    var commandsFromThisFile = await TalonServiceField.ImportTalonFileContentAsync(content, file.Name);
                    totalCommandsImported += commandsFromThisFile;
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
                var totalCommandsImported = await TalonServiceField.ImportAllTalonFilesWithProgressAsync(DirectoryPath, 
                    (filesProcessed, totalFiles, commandsSoFar) =>
                    {
                        ImportProgress = filesProcessed;
                        StateHasChanged();
                    });
                
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
                var listsImported = await TalonServiceField.ImportTalonListsFromFileAsync(ListsFilePath);
                ImportResult = $"Successfully imported {listsImported} list items from {Path.GetFileName(ListsFilePath)}.";
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
