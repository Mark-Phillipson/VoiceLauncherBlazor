
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Services
{
    public interface ICssPropertyDataService
    {
        Task<List<CssPropertyDTO>> GetAllCssPropertiesAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<List<CssPropertyDTO>> SearchCssPropertiesAsync(string serverSearchTerm);
        Task<CssPropertyDTO?> AddCssProperty(CssPropertyDTO cssPropertyDTO);
        Task<CssPropertyDTO?> GetCssPropertyById(int Id);
        Task<CssPropertyDTO?> UpdateCssProperty(CssPropertyDTO cssPropertyDTO, string username);
        Task DeleteCssProperty(int Id);
        Task<int> GetTotalCount();
    }
}