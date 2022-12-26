
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface IApplicationDetailDataService
    {
        Task<List<ApplicationDetailDTO>> GetAllApplicationDetailsAsync( );
        Task<List<ApplicationDetailDTO>> SearchApplicationDetailsAsync(string serverSearchTerm);
        Task<ApplicationDetailDTO> AddApplicationDetail(ApplicationDetailDTO applicationDetailDTO);
        Task<ApplicationDetailDTO> GetApplicationDetailById(int Id);
        Task<ApplicationDetailDTO> UpdateApplicationDetail(ApplicationDetailDTO applicationDetailDTO, string username);
        Task DeleteApplicationDetail(int Id);
    }
}
