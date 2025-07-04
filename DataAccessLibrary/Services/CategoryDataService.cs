using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;


namespace DataAccessLibrary.Services;

public class CategoryDataService : ICategoryDataService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryDataService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<List<CategoryDTO>> GetAllCategoriesByTypeAsync(string categoryType)
    {
        var Categories = await _categoryRepository.GetAllCategoriesByTypeAsync(categoryType);
        return Categories.ToList();
    }
    public async Task<List<CategoryDTO>> GetAllCategoriesAsync(string categoryType, int languageId)
    {
        var Categories = await _categoryRepository.GetAllCategoriesAsync(300, categoryType, languageId);
        return Categories.ToList();
    }
    public async Task<List<CategoryGroupedByLanguageDTO>> GetCategoriesGroupedByLanguageAsync(string categoryType)
    {
        var Categories = await _categoryRepository.GetCategoriesGroupedByLanguageAsync(categoryType);
        return Categories.ToList();
    }
    public async Task<List<CategoryDTO>> SearchCategoriesAsync(string serverSearchTerm)
    {
        var Categories = await _categoryRepository.SearchCategoriesAsync(serverSearchTerm);
        return Categories.ToList();
    }

    public async Task<CategoryDTO?> GetCategoryById(int Id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(Id);
        return category;
    }
    public async Task<string> AddCategory(CategoryDTO categoryDTO)
    {
        Guard.Against.Null(categoryDTO);
        var result = await _categoryRepository.AddCategoryAsync(categoryDTO);
        if (result == null)
        {
            throw new Exception($"Add of category failed ID: {categoryDTO.Id}");
        }
        return result;
    }
    public async Task<CategoryDTO?> UpdateCategory(CategoryDTO categoryDTO, string username)
    {
        Guard.Against.Null(categoryDTO);
        Guard.Against.Null(username);
        var result = await _categoryRepository.UpdateCategoryAsync(categoryDTO);
        if (result == null)
        {
            throw new Exception($"Update of category failed ID: {categoryDTO.Id}");
        }
        return result;
    }

    public async Task DeleteCategory(int Id)
    {
        await _categoryRepository.DeleteCategoryAsync(Id);
    }
}