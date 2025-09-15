using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace TalonVoiceCommandsServer.Services;
    public interface ITalonVoiceCommandDataService
    {
    Task<int> ImportFromTalonFilesAsync(string rootFolder);
    Task<List<Models.TalonVoiceCommand>> GetAllCommandsForFiltersAsync();
    Task<TalonVoiceCommandDataService.FilterValues> GetFilterValuesFromJavaScriptAsync();
    Task<TalonVoiceCommandDataService.DataStatistics> GetDataStatisticsFromJavaScriptAsync();
    Task<List<Models.TalonVoiceCommand>> GetFilteredCommandsInMemory(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0);
    Task<List<Models.TalonVoiceCommand>> SearchFilteredCommandsByIdsAsync(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0);
    Task<List<Models.TalonVoiceCommand>> SearchFilteredCommandsAsync(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0);
    Task<int> SearchAndDisplayDirectlyAsync(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0,
        int maxResults = 500);
    Task<List<Models.TalonVoiceCommand>> SearchFilteredCommandsSimpleAsync(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0,
        int maxResults = 500);
    Task<(List<Models.TalonVoiceCommand> Commands, int TotalMatches, bool Truncated)> SearchFilteredCommandsLimitedAsync(
        string? searchTerm = null,
        string? application = null,
        string? mode = null,
        string? operatingSystem = null,
        string? repository = null,
        string? tags = null,
        string? title = null,
        string? codeLanguage = null,
        bool useSemanticMatching = false,
        int searchScope = 0,
        int maxResults = 500);
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
    
    // IndexedDB methods for large dataset support
    Task EnsureLoadedFromIndexedDBAsync(IJSRuntime? jsRuntime);
    Task MigrateToIndexedDBAsync(IJSRuntime? jsRuntime);
    Task SaveToIndexedDBAsync(IJSRuntime? jsRuntime);
    }
