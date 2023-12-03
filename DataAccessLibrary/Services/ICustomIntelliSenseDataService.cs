using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Services
{
    public interface ICustomIntelliSenseDataService
    {
        Task<List<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync( int LanguageId,int CategoryId, int pageNumber, int pageSize);
        Task<List<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm);
        Task<CustomIntelliSenseDTO> AddCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO);
        Task<CustomIntelliSenseDTO> GetCustomIntelliSenseById(int Id);
        Task<CustomIntelliSenseDTO> UpdateCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO, string? username);
        Task DeleteCustomIntelliSense(int Id);
    }
}
