using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Services
{
    public interface IMicrophoneDataService
    {
        Task<List<MicrophoneDTO>> GetAllMicrophonesAsync();
        Task<List<MicrophoneDTO>> SearchMicrophonesAsync(string serverSearchTerm);
        Task<MicrophoneDTO> AddMicrophone(MicrophoneDTO microphoneDTO);
        Task<MicrophoneDTO> GetMicrophoneById(int Id);
        Task<MicrophoneDTO> UpdateMicrophone(MicrophoneDTO microphoneDTO, string username);
        Task DeleteMicrophone(int Id);
    }
}
