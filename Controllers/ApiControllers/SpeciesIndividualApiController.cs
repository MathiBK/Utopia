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
    public class SpeciesIndividualApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SpeciesIndividualApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SpeciesIndividualApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpeciesIndividual>>> GetSpeciesIndividuals()
        {
            return await _context.SpeciesIndividuals.ToListAsync();
        }

        // GET: api/SpeciesIndividualApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpeciesIndividual>> GetSpeciesIndividual(int id)
        {
            var speciesIndividual = await _context.SpeciesIndividuals.FindAsync(id);

            if (speciesIndividual == null)
            {
                return NotFound();
            }

            return speciesIndividual;
        }

        // PUT: api/SpeciesIndividualApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpeciesIndividual(int id, SpeciesIndividual speciesIndividual)
        {
            if (id != speciesIndividual.Id)
            {
                return BadRequest();
            }

            _context.Entry(speciesIndividual).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpeciesIndividualExists(id))
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

        // POST: api/SpeciesIndividualApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<SpeciesIndividual>> PostSpeciesIndividual(SpeciesIndividual speciesIndividual)
        {
            _context.SpeciesIndividuals.Add(speciesIndividual);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpeciesIndividual", new { id = speciesIndividual.Id }, speciesIndividual);
        }

        // DELETE: api/SpeciesIndividualApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SpeciesIndividual>> DeleteSpeciesIndividual(int id)
        {
            var speciesIndividual = await _context.SpeciesIndividuals.FindAsync(id);
            if (speciesIndividual == null)
            {
                return NotFound();
            }

            _context.SpeciesIndividuals.Remove(speciesIndividual);
            await _context.SaveChangesAsync();

            return speciesIndividual;
        }

        private bool SpeciesIndividualExists(int id)
        {
            return _context.SpeciesIndividuals.Any(e => e.Id == id);
        }
    }
}
