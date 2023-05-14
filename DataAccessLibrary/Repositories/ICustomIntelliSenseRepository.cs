
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface ICustomIntelliSenseRepository
    {
        Task<CustomIntelliSenseDTO> AddCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO);
        Task DeleteCustomIntelliSenseAsync(int Id);
		Task<IEnumerable<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync(int CategoryID);
        Task<IEnumerable<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm);
        Task<CustomIntelliSenseDTO> GetCustomIntelliSenseByIdAsync(int Id);
        Task<CustomIntelliSenseDTO> UpdateCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO);
    }
}