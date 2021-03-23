using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public class ComputerService
    {
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public ComputerService(IDbContextFactory<ApplicationDbContext> context)
        {
            _contextFactory = context;
        }
        public async Task<List<Computer>> GetComputersAsync()
        {
			using var context = _contextFactory.CreateDbContext();
			List<Computer> computers = await context.Computers.OrderBy(v => v.ComputerName).ToListAsync();
            return computers;
        }
    }
}
