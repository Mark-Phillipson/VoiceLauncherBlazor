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
    public class LaunchersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LaunchersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Launchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Launcher>>> GetLauncher()
        {
            return await _context.Launcher.ToListAsync();
        }

        // GET: api/Launchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Launcher>> GetLauncher(int id)
        {
            var launcher = await _context.Launcher.FindAsync(id);

            if (launcher == null)
            {
                return NotFound();
            }

            return launcher;
        }

        // PUT: api/Launchers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLauncher(int id, Launcher launcher)
        {
            if (id != launcher.Id)
            {
                return BadRequest();
            }

            _context.Entry(launcher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LauncherExists(id))
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

        // POST: api/Launchers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Launcher>> PostLauncher(Launcher launcher)
        {
            _context.Launcher.Add(launcher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLauncher", new { id = launcher.Id }, launcher);
        }

        // DELETE: api/Launchers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Launcher>> DeleteLauncher(int id)
        {
            var launcher = await _context.Launcher.FindAsync(id);
            if (launcher == null)
            {
                return NotFound();
            }

            _context.Launcher.Remove(launcher);
            await _context.SaveChangesAsync();

            return launcher;
        }

        private bool LauncherExists(int id)
        {
            return _context.Launcher.Any(e => e.Id == id);
        }
    }
}
