using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DataAccessLibrary.Services
{
    public interface ITalonVoiceCommandDataService
    {
    Task<int> ImportFromTalonFilesAsync(string rootFolder);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> GetAllCommandsForFiltersAsync();
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> SemanticSearchAsync(string searchTerm);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> SemanticSearchWithListsAsync(string searchTerm);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm);
    Task<List<DataAccessLibrary.Models.TalonVoiceCommand>> SearchAllAsync(string searchTerm);
    Task<List<DataAccessLibrary.Models.TalonList>> GetListContentsAsync(string listName);
    Task<int> ImportTalonFileContentAsync(string fileContent, string fileName);
    Task<int> ImportAllTalonFilesWithProgressAsync(string rootFolder, Action<int, int, int>? progressCallback = null);
    Task<int> ImportTalonListsFromFileAsync(string filePath);
    Task<List<DataAccessLibrary.DTO.CommandsBreakdown>> GetTalonCommandsBreakdownAsync();
    Task<List<DataAccessLibrary.Models.TalonList>> GetAllTalonListsAsync();
    }
}
