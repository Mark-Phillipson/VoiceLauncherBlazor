using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
    public interface ICustomWindowsSpeechCommandDataService
    {
        Task<List<CustomWindowsSpeechCommandDTO>> GetAllCustomWindowsSpeechCommandsAsync(int windowsSpeechVoiceCommandId);
        Task<List<CustomWindowsSpeechCommandDTO>> SearchCustomWindowsSpeechCommandsAsync(string serverSearchTerm);
        Task<CustomWindowsSpeechCommandDTO?> AddCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO);
        Task<CustomWindowsSpeechCommandDTO?> GetCustomWindowsSpeechCommandById(int Id);
        Task<CustomWindowsSpeechCommandDTO?> UpdateCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO, string username);
        Task DeleteCustomWindowsSpeechCommand(int Id);
    }
}
