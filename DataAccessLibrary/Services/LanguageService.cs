using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public class LanguageService
    {
        private readonly ApplicationDbContext _context;
        public LanguageService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Language>> GetLanguagesAsync(string searchTerm = null, string sortColumn = null, string sortType = null, bool? activeFilter = null, int maximumRows = 200)
        {
            IQueryable<Language> languages = null;
            try
            {
                languages = _context.Languages.Include(i => i.CustomIntelliSense).OrderBy(v => v.LanguageName);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
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
        public async Task<Language> GetLanguageAsync(int languageId)
        {
            Language language = await _context.Languages.Include("CustomIntelliSense").Where(v => v.Id == languageId).FirstOrDefaultAsync();
            return language;
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
        public async Task<List<Language>> SaveAllLanguages(List<Language> languages)
        {
            foreach (var language in languages)
            {
                await SaveLanguage(language);
            }
            return languages;
        }


    }
}
