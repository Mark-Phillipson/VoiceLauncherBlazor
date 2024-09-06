using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories;

public interface ICustomIntelliSenseRepository
{
   Task<CustomIntelliSenseDTO?> AddCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO);
   Task DeleteCustomIntelliSenseAsync(int Id);
   Task<IEnumerable<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync(int LanguageId, int CategoryId, int pageNumber, int pageSize);
   Task<IEnumerable<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm);
   Task<CustomIntelliSenseDTO?> GetCustomIntelliSenseByIdAsync(int Id);
   Task<CustomIntelliSenseDTO?> UpdateCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO);
}