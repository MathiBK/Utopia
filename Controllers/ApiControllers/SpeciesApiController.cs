using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using utopia.Data;
using utopia.Models;

namespace utopia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeciesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpeciesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpeciesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Species>>> GetSpecies()
        {
            return await _context.Species.ToListAsync();
        }

        // GET: api/SpeciesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Species>> GetSpecies(int id)
        {
            var species = await _context.Species.FindAsync(id);

            if (species == null)
            {
                return NotFound();
            }

            return species;
        }
        
        
        

        // PUT: api/SpeciesApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecies(int id, Species species)
        {
            if (id != species.Id)
            {
                return BadRequest();
            }

            _context.Entry(species).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpeciesExists(id))
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

        // POST: api/SpeciesApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Species>> PostSpecies(Species species)
        {
            _context.Species.Add(species);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpecies", new { id = species.Id }, species);
        }

        // DELETE: api/SpeciesApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Species>> DeleteSpecies(int id)
        {
            var species = await _context.Species.FindAsync(id);
            if (species == null)
            {
                return NotFound();
            }

            _context.Species.Remove(species);
            await _context.SaveChangesAsync();

            return species;
        }

        private bool SpeciesExists(int id)
        {
            return _context.Species.Any(e => e.Id == id);
        }
    }
}
