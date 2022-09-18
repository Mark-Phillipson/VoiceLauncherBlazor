
using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
    public interface IWindowsSpeechVoiceCommandDataService
    {
        Task<List<WindowsSpeechVoiceCommandDTO>> GetAllWindowsSpeechVoiceCommandsAsync( bool showAutoCreated);
        Task<List<WindowsSpeechVoiceCommandDTO>> SearchWindowsSpeechVoiceCommandsAsync(string serverSearchTerm);
        Task<WindowsSpeechVoiceCommandDTO> AddWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO);
        Task<WindowsSpeechVoiceCommandDTO> GetWindowsSpeechVoiceCommandById(int Id);
        Task<WindowsSpeechVoiceCommandDTO> UpdateWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO, string username);
        Task DeleteWindowsSpeechVoiceCommand(int Id);
        Task<WindowsSpeechVoiceCommandDTO> GetLatestAdded();
    }
}
