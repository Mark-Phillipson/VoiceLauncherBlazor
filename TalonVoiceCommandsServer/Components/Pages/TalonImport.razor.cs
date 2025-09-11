using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TalonVoiceCommandsServer.Components.Pages;

public partial class TalonImport : ComponentBase
{
    // JSRuntime is injected in the `.razor` file via @inject and is available on the partial class

    private const string SettingsDirectoryKey = "TalonImport.DirectoryPath";
    private const string SettingsListsFileKey = "TalonImport.ListsFilePath";
    private const string SettingsLockedKey = "TalonImport.SettingsLocked";

    /// <summary>
    /// When true the Directory/List textboxes and Save button are disabled and cannot be changed.
    /// This value is persisted to localStorage so it survives restarts of the app in the browser.
    /// </summary>
    public bool SettingsLocked { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            if (JSRuntime != null)
            {
                var dir = await JSRuntime.InvokeAsync<string>("localStorage.getItem", SettingsDirectoryKey);
                if (!string.IsNullOrEmpty(dir))
                {
                    DirectoryPath = dir;
                }

                var lists = await JSRuntime.InvokeAsync<string>("localStorage.getItem", SettingsListsFileKey);
                if (!string.IsNullOrEmpty(lists))
                {
                    ListsFilePath = lists;
                }

                var locked = await JSRuntime.InvokeAsync<string>("localStorage.getItem", SettingsLockedKey);
                if (!string.IsNullOrEmpty(locked) && bool.TryParse(locked, out var parsed))
                {
                    SettingsLocked = parsed;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading TalonImport settings: {ex.Message}");
        }
    }

    protected async Task SaveSettingsAsync()
    {
        try
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", SettingsDirectoryKey, DirectoryPath ?? string.Empty);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", SettingsListsFileKey, ListsFilePath ?? string.Empty);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", SettingsLockedKey, SettingsLocked.ToString());
            }
            // Persisted - now validate existence so the user gets immediate feedback if a path is incorrect
            var missing = new List<string>();
            if (!string.IsNullOrWhiteSpace(DirectoryPath) && !Directory.Exists(DirectoryPath))
            {
                missing.Add($"Directory does not exist: {DirectoryPath}");
            }
            if (!string.IsNullOrWhiteSpace(ListsFilePath) && !File.Exists(ListsFilePath))
            {
                missing.Add($"Lists file does not exist: {ListsFilePath}");
            }

            if (missing.Count > 0)
            {
                ImportResult = "Settings saved. One or more paths do not exist.";
                ErrorMessage = string.Join("; ", missing);
            }
            else
            {
                ImportResult = "Settings saved.";
                ErrorMessage = null;
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error saving settings: " + ex.Message;
        }
    }

    protected async Task SaveLockStateAsync()
    {
        try
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", SettingsLockedKey, SettingsLocked.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving lock state: {ex.Message}");
        }
    }

    public class RepoItem
    {
        public string Name { get; set; } = string.Empty;
        public int TalonFileCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public List<RepoItem>? Repos { get; set; }

    protected Task PickUserFolderAndListRepos()
    {
        try
        {
            ImportResult = null;
            ErrorMessage = null;
            IsLoading = true;
            StateHasChanged();

            if (string.IsNullOrWhiteSpace(DirectoryPath) || !Directory.Exists(DirectoryPath))
            {
                ErrorMessage = "Please enter a valid directory path.";
                Repos = null;
                return Task.CompletedTask;
            }

            var rootName = Path.GetFileName(DirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var list = new List<RepoItem>();

            // Count talon files in root
            int rootCount = Directory.EnumerateFiles(DirectoryPath, "*.talon", SearchOption.TopDirectoryOnly).Count();
            list.Add(new RepoItem { Name = rootName, TalonFileCount = rootCount, IsSelected = false });

            foreach (var dir in Directory.GetDirectories(DirectoryPath))
            {
                try
                {
                    var name = Path.GetFileName(dir);
                    var count = Directory.EnumerateFiles(dir, "*.talon", SearchOption.TopDirectoryOnly).Count();
                    list.Add(new RepoItem { Name = name, TalonFileCount = count, IsSelected = false });
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Skipping directory due to error: {ex.Message}");
                }
            }

            Repos = list;
        }
        catch (System.Exception ex)
        {
            ErrorMessage = "Error picking folder: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    protected async Task ImportSelectedRepos()
    {
        if (Repos == null) return;
        var selected = Repos.Where(r => r.IsSelected).Select(r => r.Name).ToArray();
        if (selected.Length == 0)
        {
            ErrorMessage = "Please select one or more repositories to import.";
            return;
        }

        IsLoading = true;
        ImportResult = null;
        ErrorMessage = null;
        ImportProgress = 0;
        ImportTotal = 0;
        StateHasChanged();

        try
        {
            if (string.IsNullOrWhiteSpace(DirectoryPath) || !Directory.Exists(DirectoryPath))
            {
                ErrorMessage = "Please provide a valid base directory path for the repositories.";
                return;
            }

            var files = new List<(string Name, string Text)>();
            foreach (var repoName in selected)
            {
                string repoPath;
                var rootName = Path.GetFileName(DirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (string.Equals(repoName, rootName, System.StringComparison.OrdinalIgnoreCase))
                    repoPath = DirectoryPath;
                else
                    repoPath = Path.Combine(DirectoryPath, repoName);

                if (Directory.Exists(repoPath))
                {
                    foreach (var filePath in Directory.EnumerateFiles(repoPath, "*.talon", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var txt = await File.ReadAllTextAsync(filePath);
                            files.Add((Path.GetFileName(filePath), txt));
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine($"Skipping file {filePath}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Repository path does not exist: {repoPath}");
                }
            }

            ImportTotal = files.Count;
            int processed = 0;
            int totalCommandsImported = 0;
            foreach (var f in files)
            {
                try
                {
                    totalCommandsImported += await TalonVoiceCommandDataService.ImportTalonFileContentAsync(f.Text, f.Name);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error importing file {f.Name}: {ex.Message}");
                }
                processed++;
                ImportProgress = processed;
                StateHasChanged();
            }

            ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {ImportTotal} file(s) in selected repositories.";
        }
        catch (System.Exception ex)
        {
            ErrorMessage = "Error importing repositories: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    public IReadOnlyList<IBrowserFile>? SelectedFiles { get; set; }
    public bool IsLoading { get; set; } = false;
    public string? ImportResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DirectoryPath { get; set; } = @"C:\Users\MPhil\AppData\Roaming\talon\user";
    public string? ListsFilePath { get; set; } = @"C:\Users\MPhil\AppData\Roaming\talon\user\mystuff\talon_my_stuff\TalonLists.txt";
    public int ImportProgress { get; set; } = 0;
    public int ImportTotal { get; set; } = 0;
    public bool ShowToast { get; set; } = false;


    

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
                var commandsFromThisFile = await TalonVoiceCommandDataService.ImportTalonFileContentAsync(content, file.Name);
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
    }
    protected async Task ImportAllFromDirectory()
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
        {
            ErrorMessage = "Please enter a directory path.";
            return;
        }
        if (!Directory.Exists(DirectoryPath))
        {
            ErrorMessage = $"Directory does not exist: {DirectoryPath}";
            return;
        }
        IsLoading = true;
        ImportResult = null;
        ErrorMessage = null;
        ImportProgress = 0;
        ImportTotal = 0;
        try
        {
            // Use server-side import to enumerate and import all .talon files under the directory
            int totalCommandsImported = await TalonVoiceCommandDataService.ImportAllTalonFilesWithProgressAsync(DirectoryPath,
                (filesProcessed, totalFiles, commandsSoFar) =>
                {
                    ImportProgress = filesProcessed;
                    ImportTotal = totalFiles;
                    StateHasChanged();
                });

            ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {ImportTotal} .talon files (server).";
        }
        catch (Exception ex)
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

        if (!File.Exists(ListsFilePath))
        {
            ErrorMessage = $"Lists file does not exist: {ListsFilePath}";
            return;
        }

        IsLoading = true;
        ImportResult = null;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            var listsImported = await TalonVoiceCommandDataService.ImportTalonListsFromFileAsync(ListsFilePath);
            ImportResult = $"Successfully imported {listsImported} list items from {Path.GetFileName(ListsFilePath)}.";
            // Show toast in UI
            ShowToast = true;
            StateHasChanged();
            _ = Task.Run(async () =>
            {
                await Task.Delay(4000);
                ShowToast = false;
                await InvokeAsync(StateHasChanged);
            });
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
// Note: IBrowserFile does not provide the full path for security reasons. For browser uploads, only the filename is available. For server-side directory imports, the full path is already used.
