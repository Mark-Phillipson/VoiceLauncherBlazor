using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class VoiceLauncherService
    {
        readonly ApplicationDbContext _context;
        public VoiceLauncherService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<List<Category>> GetCategoriesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryTypeFilter = null, int maximumRows = 200)
        {
            //var categories = await _context.Categories.Where(v => v.CategoryType == "IntelliSense Command").OrderBy(v => v.CategoryName).ToListAsync();
            IQueryable<Category> categories = _context.Categories.Include(i => i.CustomIntelliSense).Include(i => i.Launcher).OrderBy(v => v.CategoryName);
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
        public async Task<List<Language>> GetLanguagesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, bool? activeFilter = null, int maximumRows = 200)
        {
            IQueryable<Language> languages = _context.Languages.Include(i => i.CustomIntelliSense).OrderBy(v => v.LanguageName);
            if (searchTerm != null && searchTerm.Length > 0)
            {
                languages = languages.Where(v => v.LanguageName.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "LanguageName" && sortType == "Ascending")
                {
                    languages = languages.OrderBy(v => v.LanguageName);
                }
                else if (sortColumn == "LanguageName" && sortType == "Descending")
                {
                    languages = languages.OrderByDescending(v => v.LanguageName);
                }
            }
            if (activeFilter != null)
            {
                languages = languages.Where(v => v.Active == activeFilter);
            }

            return await languages.Take(maximumRows).ToListAsync();
        }
        public async Task<List<CustomIntelliSense>> GetCustomIntelliSensesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, int? categoryIdFilter = null, int? languageIdFilter = null, int maximumRows = 200)
        {
            IQueryable<CustomIntelliSense> intellisenses = _context.CustomIntelliSense.Include(i => i.Language).OrderBy(v => v.Category);
            if (searchTerm != null && searchTerm.Length > 0)
            {
                intellisenses = intellisenses.Where(v => v.DisplayValue.Contains(searchTerm) || v.SendKeysValue.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "LanguageName" && sortType == "Ascending")
                {
                    intellisenses = intellisenses.OrderBy(v => v.Language.LanguageName);
                }
                else if (sortColumn == "LanguageName" && sortType == "Descending")
                {
                    intellisenses = intellisenses.OrderByDescending(v => v.Language.LanguageName);
                }
            }
            if (languageIdFilter != null)
            {
                intellisenses = intellisenses.Where(v => v.LanguageId == languageIdFilter);
            }
            if (categoryIdFilter != null)
            {
                intellisenses = intellisenses.Where(v => v.CategoryId == categoryIdFilter);
            }

            return await intellisenses.Take(maximumRows).ToListAsync();
        }
        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            Category category = await _context.Categories.Include("CustomIntelliSense.Language").Include("Launcher.Computer").Where(v => v.Id == categoryId).FirstOrDefaultAsync();
            return category;
        }
        public async Task<Language> GetLanguageAsync(int languageId)
        {
            Language language = await _context.Languages.Include("CustomIntelliSense").Where(v => v.Id == languageId).FirstOrDefaultAsync();
            return language;
        }
        public async Task<CustomIntelliSense> GetIntelliSenseAsync(int intellisenseId)
        {
            CustomIntelliSense intellisense = await _context.CustomIntelliSense.Include(i => i.Language).Include(i => i.Category).Where(v => v.Id == intellisenseId).FirstOrDefaultAsync();
            return intellisense;
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
        public async Task<string> SaveLanguage(Language language)
        {
            if (language.Id > 0)
            {
                _context.Languages.Update(language);
            }
            else
            {
                _context.Languages.Add(language);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"Language Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public async Task<string> SaveCustomIntelliSense(CustomIntelliSense intellisense)
        {
            if (intellisense.Id > 0)
            {
                _context.CustomIntelliSense.Update(intellisense);
            }
            else
            {
                _context.CustomIntelliSense.Add(intellisense);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"Custom IntelliSense Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
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
        public async Task<string> DeleteLanguage(int languageId)
        {
            var language = await _context.Languages.Where(v => v.Id == languageId).FirstOrDefaultAsync();
            var result = $"Delete Language Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (language != null)
            {
                _context.Languages.Remove(language);
                await _context.SaveChangesAsync();
                result = $"Language Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            return result;
        }
        public async Task<string> DeleteCustomIntelliSense(int customIntellisenseId)
        {
            var intellisense = await _context.CustomIntelliSense.Where(v => v.Id == customIntellisenseId).FirstOrDefaultAsync();
            var result = $"Delete Custom IntelliSense Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (intellisense != null)
            {
                _context.CustomIntelliSense.Remove(intellisense);
                await _context.SaveChangesAsync();
                result = $"Custom IntelliSense Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
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
        public async Task<List<Language>> SaveAllLanguages(List<Language> languages)
        {
            foreach (var language in languages)
            {
                await SaveLanguage(language);
            }
            return languages;
        }
        public async Task<List<CustomIntelliSense>> SaveAllCustomIntelliSenses(List<CustomIntelliSense> intellisenses)
        {
            foreach (var intellisense in intellisenses)
            {
                await SaveCustomIntelliSense(intellisense);
            }
            return intellisenses;
        }
        public async Task<List<GeneralLookup>> GetGeneralLookUpsAsync(string category)
        {
            List<GeneralLookup> generalLookups = await _context.GeneralLookups.Where(v => v.Category == category).OrderBy(v => v.SortOrder).ToListAsync();
            return generalLookups;
        }
    }
}
