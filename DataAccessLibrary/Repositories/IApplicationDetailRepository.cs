
using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Repositories
{
    public interface IApplicationDetailRepository
    {
        Task<ApplicationDetailDTO?> AddApplicationDetailAsync(ApplicationDetailDTO applicationDetailDTO);
        Task DeleteApplicationDetailAsync(int Id);
        Task<IEnumerable<ApplicationDetailDTO>> GetAllApplicationDetailsAsync(int maxRows);
        Task<IEnumerable<ApplicationDetailDTO>> SearchApplicationDetailsAsync(string serverSearchTerm);
        Task<ApplicationDetailDTO?> GetApplicationDetailByIdAsync(int Id);
        Task<ApplicationDetailDTO?> UpdateApplicationDetailAsync(ApplicationDetailDTO applicationDetailDTO);
    }
}