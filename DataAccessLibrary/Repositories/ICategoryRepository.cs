
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface ICategoryRepository
    {
        Task<CategoryDTO> AddCategoryAsync(CategoryDTO categoryDTO);
        Task DeleteCategoryAsync(int Id);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(int maxRows, string categoryType);
        Task<IEnumerable<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm);
        Task<CategoryDTO> GetCategoryByIdAsync(int Id);
        Task<CategoryDTO> UpdateCategoryAsync(CategoryDTO categoryDTO);
    }
}