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
    public class CustomIntelliSensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomIntelliSensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CustomIntelliSenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomIntelliSense>>> GetCustomIntelliSense()
        {
            return await _context.CustomIntelliSense.ToListAsync();
        }

        // GET: api/CustomIntelliSenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomIntelliSense>> GetCustomIntelliSense(int id)
        {
            var customIntelliSense = await _context.CustomIntelliSense.FindAsync(id);

            if (customIntelliSense == null)
            {
                return NotFound();
            }

            return customIntelliSense;
        }

        // PUT: api/CustomIntelliSenses/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomIntelliSense(int id, CustomIntelliSense customIntelliSense)
        {
            if (id != customIntelliSense.Id)
            {
                return BadRequest();
            }

            _context.Entry(customIntelliSense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomIntelliSenseExists(id))
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

        // POST: api/CustomIntelliSenses
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<CustomIntelliSense>> PostCustomIntelliSense(CustomIntelliSense customIntelliSense)
        {
            _context.CustomIntelliSense.Add(customIntelliSense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomIntelliSense", new { id = customIntelliSense.Id }, customIntelliSense);
        }

        // DELETE: api/CustomIntelliSenses/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CustomIntelliSense>> DeleteCustomIntelliSense(int id)
        {
            var customIntelliSense = await _context.CustomIntelliSense.FindAsync(id);
            if (customIntelliSense == null)
            {
                return NotFound();
            }

            _context.CustomIntelliSense.Remove(customIntelliSense);
            await _context.SaveChangesAsync();

            return customIntelliSense;
        }

        private bool CustomIntelliSenseExists(int id)
        {
            return _context.CustomIntelliSense.Any(e => e.Id == id);
        }
    }
}
