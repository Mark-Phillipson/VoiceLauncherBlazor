
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IMicrophoneRepository
    {
        Task<MicrophoneDTO> AddMicrophoneAsync(MicrophoneDTO microphoneDTO);
        Task DeleteMicrophoneAsync(int Id);
        Task<IEnumerable<MicrophoneDTO>> GetAllMicrophonesAsync(int maxRows);
        Task<IEnumerable<MicrophoneDTO>> SearchMicrophonesAsync(string serverSearchTerm);
        Task<MicrophoneDTO> GetMicrophoneByIdAsync(int Id);
        Task<MicrophoneDTO> UpdateMicrophoneAsync(MicrophoneDTO microphoneDTO);
    }
}