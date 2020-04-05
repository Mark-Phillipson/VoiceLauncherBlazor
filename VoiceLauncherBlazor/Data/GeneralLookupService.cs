using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class GeneralLookupService
    {
        private readonly ApplicationDbContext _context;
        public GeneralLookupService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<GeneralLookup>> GetGeneralLookUpsAsync(string category)
        {
            List<GeneralLookup> generalLookups = await _context.GeneralLookups.Where(v => v.Category == category).OrderBy(v => v.SortOrder).ToListAsync();
            return generalLookups;
        }
        public async Task<List<string>> GetGeneralLookUpsCategoriesAsync()
        {
            List<string> collection = await _context.GeneralLookups.OrderBy(c => c.Category).GroupBy(g => g.Category).Select(s => s.Key).ToListAsync();
            return collection;
        }
        public async Task<List<GeneralLookup>> GetGeneralLookupsAsync(string searchTerm = null, string sortColumn = null, string sortType = null, string categoryFilter = null, int maximumRows = 200)
        {
            IQueryable<GeneralLookup> generalLookups = null;
            try
            {
                generalLookups = _context.GeneralLookups.OrderBy(v => v.Category).ThenBy(t => t.SortOrder);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            if (searchTerm != null && searchTerm.Length > 0)
            {
                generalLookups = generalLookups.Where(v => v.ItemValue.Contains(searchTerm) || v.Category.Contains(searchTerm) || v.DisplayValue.Contains(searchTerm));
            }
            if (sortType != null && sortColumn != null)
            {
                if (sortColumn == "ItemValue" && sortType == "Ascending")
                {
                    generalLookups = generalLookups.OrderBy(v => v.ItemValue);
                }
                else if (sortColumn == "ItemValue" && sortType == "Descending")
                {
                    generalLookups = generalLookups.OrderByDescending(v => v.ItemValue);
                }
            }
            if (categoryFilter != null)
            {
                generalLookups = generalLookups.Where(v => v.Category == categoryFilter);
            }
            return await generalLookups.Take(maximumRows).ToListAsync();
        }
        public async Task<string> DeleteGeneralLookup(int generalLookupId)
        {
            var generalLookup = await _context.GeneralLookups.Where(v => v.Id == generalLookupId).FirstOrDefaultAsync();
            var result = $"Delete General Lookup Failed {DateTime.UtcNow:h:mm:ss tt zz}";
            if (generalLookup != null)
            {
                _context.GeneralLookups.Remove(generalLookup);
                await _context.SaveChangesAsync();
                result = $"General Lookup Successfully Deleted {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            return result;
        }
        public async Task<List<GeneralLookup>> SaveAllGeneralLookups(List<GeneralLookup> generalLookups)
        {
            foreach (var generalLookup in generalLookups)
            {
                if (generalLookup.Id == 0)
                {
                    if (generalLookup.ItemValue != "*" && generalLookup.ItemValue?.Length > 0 && generalLookup.Category?.Length > 0)
                    {
                        _context.GeneralLookups.Add(generalLookup);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    await SaveGeneralLookup(generalLookup);
                }
            }
            return generalLookups;
        }
        public async Task<string> SaveGeneralLookup(GeneralLookup generalLookup)
        {
            generalLookup = TrimGeneralLookup(generalLookup);
            if (generalLookup.Id > 0)
            {
                _context.GeneralLookups.Update(generalLookup);
            }
            else
            {
                _context.GeneralLookups.Add(generalLookup);
            }
            try
            {
                await _context.SaveChangesAsync();
                return $"General Lookup Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private GeneralLookup TrimGeneralLookup(GeneralLookup generalLookup)
        {
            generalLookup.Category = generalLookup.Category?.Trim();
            generalLookup.ItemValue = generalLookup.ItemValue?.Trim();
            generalLookup.DisplayValue = generalLookup.DisplayValue?.Trim();
            return generalLookup;
        }

        public async Task<GeneralLookup> GetGeneralLookupAsync(int generalLookupId)
        {
            GeneralLookup generalLookup = await _context.GeneralLookups.Where(v => v.Id == generalLookupId).FirstOrDefaultAsync();
            return generalLookup;
        }
    }
}
