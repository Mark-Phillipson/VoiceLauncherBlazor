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
        public int ImportProgress { get; set; } = 0;
        public int ImportTotal { get; set; } = 0;

        protected void OnFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFiles = e.GetMultipleFiles();
            ImportResult = null;
            ErrorMessage = null;
        }

        protected async Task ImportFiles()
        {
            if (SelectedFiles == null || SelectedFiles.Count == 0)
                return;
            IsLoading = true;
            ImportResult = null;
            ErrorMessage = null;
            int imported = 0;
            try
            {
                foreach (var file in SelectedFiles)
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    await TalonServiceField.ImportTalonFileContentAsync(content, file.Name);
                    imported++;
                }
                ImportResult = $"Successfully imported {imported} file(s).";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error importing files: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task ImportAllFromDirectory()
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
                int imported = 0;
                var context = (TalonServiceField as DataAccessLibrary.Services.TalonVoiceCommandDataService)?.GetType()
                    .GetProperty("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(TalonServiceField) as DataAccessLibrary.Models.ApplicationDbContext;
                if (context != null)
                {
                    context.TalonVoiceCommands.RemoveRange(context.TalonVoiceCommands);
                    context.SaveChanges();
                }
                foreach (var file in talonFiles)
                {
                    var content = await File.ReadAllTextAsync(file);
                    await TalonServiceField.ImportTalonFileContentAsync(content, file);
                    imported++;
                    ImportProgress = imported;
                    StateHasChanged();
                }
                ImportResult = $"Successfully imported {imported} commands from all .talon files in directory.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error importing from directory: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
// Note: IBrowserFile does not provide the full path for security reasons. For browser uploads, only the filename is available. For server-side directory imports, the full path is already used.
