using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface ICategoryDataService
    {
        Task<List<CategoryDTO>> GetAllCategoriesAsync( string categoryType,int languageId );
        Task<List<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm);
        Task<CategoryDTO> AddCategory(CategoryDTO categoryDTO);
        Task<CategoryDTO> GetCategoryById(int Id);
        Task<CategoryDTO> UpdateCategory(CategoryDTO categoryDTO, string username);
        Task DeleteCategory(int Id);
    }
}