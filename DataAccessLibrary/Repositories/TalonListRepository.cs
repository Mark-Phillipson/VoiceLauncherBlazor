using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public class TalonListRepository : ITalonListRepository
    {
        private readonly ApplicationDbContext _context;
        public TalonListRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TalonList>> GetAllTalonListsAsync()
        {
            return await _context.TalonLists.ToListAsync();
        }

        public async Task<int> InsertTalonListsAsync(IEnumerable<TalonList> items)
        {
            if (items == null) return 0;

            var inputs = items
                .Where(i => !string.IsNullOrWhiteSpace(i.ListName)
                    && !string.IsNullOrWhiteSpace(i.SpokenForm)
                    && !string.IsNullOrWhiteSpace(i.ListValue))
                .Select(i => new
                {
                    ListName = i.ListName.Trim(),
                    SpokenForm = i.SpokenForm.Trim(),
                    ListValue = i.ListValue.Trim(),
                    SourceFile = i.SourceFile
                })
                .GroupBy(x => (x.ListName, x.SpokenForm, x.ListValue))
                .Select(g => g.First())
                .ToList();

            if (!inputs.Any()) return 0;

            var listNames = inputs.Select(i => i.ListName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var existing = await _context.TalonLists
                .Where(t => listNames.Contains(t.ListName))
                .ToListAsync();

            var newItems = new List<TalonList>();
            foreach (var inp in inputs)
            {
                var exists = existing.Any(e =>
                    string.Equals(e.ListName, inp.ListName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.SpokenForm, inp.SpokenForm, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.ListValue, inp.ListValue, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    var now = DateTime.UtcNow;
                    newItems.Add(new TalonList
                    {
                        ListName = inp.ListName,
                        SpokenForm = inp.SpokenForm,
                        ListValue = inp.ListValue,
                        SourceFile = inp.SourceFile,
                        CreatedAt = now,
                        ImportedAt = now
                    });
                }
            }

            if (newItems.Any())
            {
                _context.TalonLists.AddRange(newItems);
                await _context.SaveChangesAsync();
                return newItems.Count;
            }

            return 0;
        }
    }
}
