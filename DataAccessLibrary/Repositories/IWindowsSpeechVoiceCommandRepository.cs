using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface IWindowsSpeechVoiceCommandRepository
    {
        Task<WindowsSpeechVoiceCommandDTO?> AddWindowsSpeechVoiceCommandAsync(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO);
        Task DeleteWindowsSpeechVoiceCommandAsync(int Id);
        Task<IEnumerable<WindowsSpeechVoiceCommandDTO>> GetAllWindowsSpeechVoiceCommandsAsync(int maxRows, bool showAutoCreated);
        IEnumerable<WindowsSpeechVoiceCommandDTO> SearchWindowsSpeechVoiceCommandsAsync(string serverSearchTerm);
        Task<WindowsSpeechVoiceCommandDTO?> GetWindowsSpeechVoiceCommandByIdAsync(int Id);
        Task<WindowsSpeechVoiceCommandDTO?> UpdateWindowsSpeechVoiceCommandAsync(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO);
        Task<WindowsSpeechVoiceCommandDTO?> GetLatestAdded();
        Task<List<ApplicationDetail>> GetAllApplicationDetails();
        Task<List<CommandsBreakdown>> GetCommandsBreakdown();
        Task<List<SpokenForm>> GetSpokenFormsAsync(List<WindowsSpeechVoiceCommandDTO> result);
    }
}