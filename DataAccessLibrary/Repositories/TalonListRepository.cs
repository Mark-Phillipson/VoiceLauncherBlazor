using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
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
    }
}
