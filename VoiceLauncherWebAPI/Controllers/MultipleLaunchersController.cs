using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLibrary.Models;

namespace VoiceLauncherWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultipleLaunchersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MultipleLaunchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MultipleLaunchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultipleLauncher>>> GetMultipleLauncher()
        {
            return await _context.MultipleLauncher.ToListAsync();
        }

        // GET: api/MultipleLaunchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MultipleLauncher>> GetMultipleLauncher(int id)
        {
            var multipleLauncher = await _context.MultipleLauncher.FindAsync(id);

            if (multipleLauncher == null)
            {
                return NotFound();
            }

            return multipleLauncher;
        }

        // PUT: api/MultipleLaunchers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMultipleLauncher(int id, MultipleLauncher multipleLauncher)
        {
            if (id != multipleLauncher.Id)
            {
                return BadRequest();
            }

            _context.Entry(multipleLauncher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MultipleLauncherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MultipleLaunchers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MultipleLauncher>> PostMultipleLauncher(MultipleLauncher multipleLauncher)
        {
            _context.MultipleLauncher.Add(multipleLauncher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMultipleLauncher", new { id = multipleLauncher.Id }, multipleLauncher);
        }

        // DELETE: api/MultipleLaunchers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MultipleLauncher>> DeleteMultipleLauncher(int id)
        {
            var multipleLauncher = await _context.MultipleLauncher.FindAsync(id);
            if (multipleLauncher == null)
            {
                return NotFound();
            }

            _context.MultipleLauncher.Remove(multipleLauncher);
            await _context.SaveChangesAsync();

            return multipleLauncher;
        }

        private bool MultipleLauncherExists(int id)
        {
            return _context.MultipleLauncher.Any(e => e.Id == id);
        }
    }
}
