
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface ICssPropertyRepository
    {
        Task<CssPropertyDTO?> AddCssPropertyAsync(CssPropertyDTO cssPropertyDTO);
        Task DeleteCssPropertyAsync(int Id);
        Task<IEnumerable<CssPropertyDTO>> GetAllCssPropertiesAsync(int pageNumber, int pageSize, string? serverSearchTerm);
        Task<IEnumerable<CssPropertyDTO>> SearchCssPropertiesAsync(string serverSearchTerm);
        Task<CssPropertyDTO?> GetCssPropertyByIdAsync(int Id);
        Task<CssPropertyDTO?> UpdateCssPropertyAsync(CssPropertyDTO cssPropertyDTO);
        Task<int> GetTotalCountAsync();
    }
}