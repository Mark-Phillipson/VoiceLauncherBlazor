using System.Collections.Generic;
using System.Threading.Tasks;
using SharedContracts.Models;

namespace SharedContracts.Services
{
    public interface ITalonVoiceCommandRepository
    {
        Task<int> ImportFromTalonFilesAsync(string rootFolder);
        Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsAsync();
        Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsForFiltersAsync();
        Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchWithListsAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandNamesOnlyAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommandDto>> SearchScriptOnlyAsync(string searchTerm);
        Task<IEnumerable<TalonVoiceCommandDto>> SearchAllAsync(string searchTerm);
        Task<IEnumerable<TalonListDto>> GetListContentsAsync(string listName);
        Task<string> ExportAllJsonAsync();
        Task ImportFromJsonAsync(string json);
        Task DeleteAllAsync();
    }
}
