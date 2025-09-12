using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace TalonVoiceCommandsServer.Services;
    public interface ITalonVoiceCommandDataService
    {
    Task<int> ImportFromTalonFilesAsync(string rootFolder);
    Task<List<Models.TalonVoiceCommand>> GetAllCommandsForFiltersAsync();
    Task<List<Models.TalonVoiceCommand>> SemanticSearchAsync(string searchTerm);
    Task<List<Models.TalonVoiceCommand>> SemanticSearchWithListsAsync(string searchTerm);
    Task<List<Models.TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm);
    Task<List<Models.TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm);
    Task<List<Models.TalonVoiceCommand>> SearchAllAsync(string searchTerm);
    Task<List<Models.TalonList>> GetListContentsAsync(string listName);
    Task<int> ImportTalonFileContentAsync(string fileContent, string fileName);
    Task<int> ImportAllTalonFilesWithProgressAsync(string rootFolder, Action<int, int, int>? progressCallback = null);
    Task<int> ImportTalonListsFromFileAsync(string filePath);
    Task<List<TalonVoiceCommandsServer.Models.TalonCommandBreakdown>> GetTalonCommandsBreakdownAsync();
    Task SaveToLocalStorageAsync(IJSRuntime jsRuntime);
    }
