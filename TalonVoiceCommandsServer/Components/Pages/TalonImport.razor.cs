using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TalonVoiceCommandsServer.Components.Pages;

public partial class TalonImport : ComponentBase
{
    public class RepoItem
    {
        public string Name { get; set; } = string.Empty;
        public int TalonFileCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public List<RepoItem>? Repos { get; set; }

    protected async Task PickUserFolderAndListRepos()
    {
        try
        {
            ImportResult = null;
            ErrorMessage = null;
            IsLoading = true;
            StateHasChanged();

            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/fs-access.js");
            var result = await module.InvokeAsync<System.Text.Json.JsonElement>("pickUserFolderAndListRepos");
            if (result.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var rootName = result.GetProperty("rootName").GetString();
                var reposArr = result.GetProperty("repos");
                var list = new List<RepoItem>();
                foreach (var je in reposArr.EnumerateArray())
                {
                    var name = je.GetProperty("name").GetString() ?? "";
                    var count = je.GetProperty("talonFileCount").GetInt32();
                    list.Add(new RepoItem { Name = name, TalonFileCount = count, IsSelected = false });
                }
                Repos = list;
            }
            else
            {
                ErrorMessage = "Failed to list repositories from selected folder.";
            }
        }
        catch (JSException jsEx)
        {
            ErrorMessage = "Browser file system API error: " + jsEx.Message;
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
            var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/fs-access.js");
            var filesElem = await module.InvokeAsync<System.Text.Json.JsonElement>("getFilesForRepos", selected);
            var files = new List<(string Name, string Text)>();
            if (filesElem.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var je in filesElem.EnumerateArray())
                {
                    try
                    {
                        var name = je.GetProperty("name").GetString() ?? "unnamed";
                        var text = je.GetProperty("text").GetString() ?? string.Empty;
                        files.Add((name, text));
                    }
                    catch { }
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
        catch (JSException jsEx)
        {
            ErrorMessage = "Browser JS error while reading files: " + jsEx.Message;
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
        IsLoading = true;
        ImportResult = null;
        ErrorMessage = null;
        ImportProgress = 0;
        ImportTotal = 0;
        try
        {
            // Try browser directory APIs (File System Access API) via JS interop first
            List<(string Name, string Text)> files = new();
            try
            {
                var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/fs-access.js");
                System.Text.Json.JsonElement root;
                try
                {
                    // Prefer pickDirectoryFiles (File System Access API)
                    root = await module.InvokeAsync<System.Text.Json.JsonElement>("pickDirectoryFiles");
                }
                catch (JSException)
                {
                    // If pickDirectoryFiles not available or fails, fall back to input-based picker
                    root = await module.InvokeAsync<System.Text.Json.JsonElement>("readDirectoryViaInput");
                }

                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var je in root.EnumerateArray())
                    {
                        try
                        {
                            var name = je.GetProperty("name").GetString() ?? "unnamed";
                            var text = je.GetProperty("text").GetString() ?? string.Empty;
                            files.Add((name, text));
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine($"Skipping file due to parse error: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("JS picker returned unexpected payload: " + root.ValueKind);
                }
            }
            catch (JSException jsEx)
            {
                // If JS import fails entirely, show an error but try server-directory fallback later
                Console.WriteLine("JS interop error: " + jsEx.Message);
            }

            ImportTotal = files.Count;
            int processed = 0;
            int totalCommandsImported = 0;
            // If JS did not return any files, attempt the server-side import fallback (if DirectoryPath provided)
            if (files.Count == 0 && !string.IsNullOrWhiteSpace(DirectoryPath))
            {
                try
                {
                    // Use existing service which should call the server API to enumerate files
                    totalCommandsImported = await TalonVoiceCommandDataService.ImportAllTalonFilesWithProgressAsync(DirectoryPath,
                        (filesProcessed, totalFiles, commandsSoFar) =>
                        {
                            ImportProgress = filesProcessed;
                            ImportTotal = totalFiles;
                            StateHasChanged();
                        });

                    ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {ImportTotal} .talon files (server fallback).";
                    return;
                }
                catch (System.Exception ex)
                {
                    ErrorMessage = "Directory import via browser failed and server fallback also failed: " + GetFullErrorMessage(ex);
                    return;
                }
            }
            foreach (var f in files)
            {
                try
                {
                    var commandsFromThisFile = await TalonVoiceCommandDataService.ImportTalonFileContentAsync(f.Text, f.Name);
                    totalCommandsImported += commandsFromThisFile;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing file {f.Name}: {ex.Message}");
                }
                processed++;
                ImportProgress = processed;
                StateHasChanged();
            }

            ImportResult = $"Successfully imported {totalCommandsImported} command(s) from {ImportTotal} file(s) in selected directory.";

            // Invalidate the filter cache so repository dropdown gets updated
            // TalonVoiceCommandSearch.InvalidateFilterCache();
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

        IsLoading = true;
        ImportResult = null;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            var listsImported = await TalonVoiceCommandDataService.ImportTalonListsFromFileAsync(ListsFilePath);
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
// Note: IBrowserFile does not provide the full path for security reasons. For browser uploads, only the filename is available. For server-side directory imports, the full path is already used.
