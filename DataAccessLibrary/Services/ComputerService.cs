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
        readonly ApplicationDbContext _context;
        public ComputerService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Computer>> GetComputersAsync()
        {
            List<Computer> computers = await _context.Computers.OrderBy(v => v.ComputerName).ToListAsync();
            return computers;
        }
    }
}
