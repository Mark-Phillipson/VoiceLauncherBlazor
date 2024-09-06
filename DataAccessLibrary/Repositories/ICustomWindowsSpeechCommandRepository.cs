
using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface ICustomWindowsSpeechCommandRepository
    {
        Task<CustomWindowsSpeechCommandDTO?> AddCustomWindowsSpeechCommandAsync(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO);
        Task DeleteCustomWindowsSpeechCommandAsync(int Id);
        Task<IEnumerable<CustomWindowsSpeechCommandDTO>> GetAllCustomWindowsSpeechCommandsAsync(int windowsSpeechVoiceCommandId);
        Task<IEnumerable<CustomWindowsSpeechCommandDTO>> SearchCustomWindowsSpeechCommandsAsync(string serverSearchTerm);
        Task<CustomWindowsSpeechCommandDTO?> GetCustomWindowsSpeechCommandByIdAsync(int Id);
        Task<CustomWindowsSpeechCommandDTO?> UpdateCustomWindowsSpeechCommandAsync(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO);
    }
}