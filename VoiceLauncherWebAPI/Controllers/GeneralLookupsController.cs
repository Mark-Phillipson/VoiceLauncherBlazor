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
    public class GeneralLookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GeneralLookupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GeneralLookups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GeneralLookup>>> GetGeneralLookups()
        {
            return await _context.GeneralLookups.ToListAsync();
        }

        // GET: api/GeneralLookups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GeneralLookup>> GetGeneralLookup(int id)
        {
            var generalLookup = await _context.GeneralLookups.FindAsync(id);

            if (generalLookup == null)
            {
                return NotFound();
            }

            return generalLookup;
        }

        // PUT: api/GeneralLookups/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGeneralLookup(int id, GeneralLookup generalLookup)
        {
            if (id != generalLookup.Id)
            {
                return BadRequest();
            }

            _context.Entry(generalLookup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeneralLookupExists(id))
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

        // POST: api/GeneralLookups
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<GeneralLookup>> PostGeneralLookup(GeneralLookup generalLookup)
        {
            _context.GeneralLookups.Add(generalLookup);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGeneralLookup", new { id = generalLookup.Id }, generalLookup);
        }

        // DELETE: api/GeneralLookups/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GeneralLookup>> DeleteGeneralLookup(int id)
        {
            var generalLookup = await _context.GeneralLookups.FindAsync(id);
            if (generalLookup == null)
            {
                return NotFound();
            }

            _context.GeneralLookups.Remove(generalLookup);
            await _context.SaveChangesAsync();

            return generalLookup;
        }

        private bool GeneralLookupExists(int id)
        {
            return _context.GeneralLookups.Any(e => e.Id == id);
        }
    }
}
