using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VoiceLauncher.Repositories
{
    public interface ICategoryRepository
    {
        Task<string> AddCategoryAsync(CategoryDTO categoryDTO);
        Task DeleteCategoryAsync(int Id);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesByTypeAsync(string categoryType);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(int maxRows, string categoryType, int
        languageId);
        Task<IEnumerable<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm);
        Task<CategoryDTO?> GetCategoryByIdAsync(int Id);
        Task<CategoryDTO?> UpdateCategoryAsync(CategoryDTO categoryDTO);
    }
}