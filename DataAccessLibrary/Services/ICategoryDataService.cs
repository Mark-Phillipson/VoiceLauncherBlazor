using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccessLibrary.Services;

public interface ICategoryDataService
{
    Task<List<CategoryDTO>> GetAllCategoriesByTypeAsync(string categoryType);
    Task<List<CategoryDTO>> GetAllCategoriesAsync(string categoryType, int languageId);
    Task<List<CategoryGroupedByLanguageDTO>> GetCategoriesGroupedByLanguageAsync(string categoryType);
    Task<List<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm);
    Task<string> AddCategory(CategoryDTO categoryDTO);
    Task<CategoryDTO?> GetCategoryById(int Id);
    Task<CategoryDTO?> UpdateCategory(CategoryDTO categoryDTO, string username);
    Task DeleteCategory(int Id);
}
