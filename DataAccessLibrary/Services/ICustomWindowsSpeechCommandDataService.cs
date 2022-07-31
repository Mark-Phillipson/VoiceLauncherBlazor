
using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Services
{
    public interface ICustomWindowsSpeechCommandDataService
    {
        Task<List<CustomWindowsSpeechCommandDTO>> GetAllCustomWindowsSpeechCommandsAsync( );
        Task<List<CustomWindowsSpeechCommandDTO>> SearchCustomWindowsSpeechCommandsAsync(string serverSearchTerm);
        Task<CustomWindowsSpeechCommandDTO> AddCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO);
        Task<CustomWindowsSpeechCommandDTO> GetCustomWindowsSpeechCommandById(int Id);
        Task<CustomWindowsSpeechCommandDTO> UpdateCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO, string username);
        Task DeleteCustomWindowsSpeechCommand(int Id);
    }
}
