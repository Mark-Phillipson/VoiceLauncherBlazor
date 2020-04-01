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

    }
}
