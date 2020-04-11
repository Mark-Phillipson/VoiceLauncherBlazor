using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class CategoryService
    {
        readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<List<Category>> GetCategoriesByTypeAsync(string categoryType)
        {
            List<Category> categories = await _context.Categories.Where(v => v.CategoryType == categoryType).OrderBy(v => v.CategoryName).ToListAsync();
            return categories;
        }
        public async Task<List<Category>> GetCategoriesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
        {
            IQueryable<Category> categories = null;
            try
            {
                categories = _context.Categories.Include(i => i.CustomIntelliSense).Include(i => i.Launcher).OrderBy(v => v.CategoryName);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            if (Environment.MachineName != "SURFACEPRO")
            {
                categories = categories.Where(v => v.Sensitive == false);
            }
            if (searchTerm != null && searchTerm.Length > 0)
            {
                categories = categories.Where(v => v.CategoryName.Contains(searchTerm) || v.CategoryType.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "CategoryName" && sortType == "Ascending")
                {
                    categories = categories.OrderBy(v => v.CategoryName);
                }
                else if (sortColumn == "CategoryName" && sortType == "Descending")
                {
                    categories = categories.OrderByDescending(v => v.CategoryName);
                }
            }
            if (categoryTypeFilter != null)
            {
                categories = categories.Where(v => v.CategoryType == categoryTypeFilter);
            }
            return await categories.Take(maximumRows).ToListAsync();
        }


        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            Category category = await _context.Categories.Include("CustomIntelliSense.Language").Include("Launcher.Computer").Where(v => v.Id == categoryId).FirstOrDefaultAsync();
            return category;
        }
        public async Task<string> SaveCategory(Category category)
        {
            if (category.Id > 0)
            {
                _context.Categories.Update(category);
            }
            else
            {
                _context.Categories.Add(category);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"Category Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task<string> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories.Where(v => v.Id == categoryId).FirstOrDefaultAsync();
            var result = $"Delete Category Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                result = $"Category Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            return result;
        }
        public async Task<List<Category>> SaveAllCategories(List<Category> categories)
        {
            foreach (var category in categories)
            {
                await SaveCategory(category);
            }
            return categories;
        }
    }
}
