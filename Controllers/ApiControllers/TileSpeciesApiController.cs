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
    public class TileSpeciesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TileSpeciesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TileSpeciesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TileSpecies>>> GetTileSpecies()
        {
            return await _context.TileSpecies.ToListAsync();
        }
        
        // GET: api/TileSpeciesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TileSpecies>> GetTileSpecies(int id)
        {
            var tileSpecies = await _context.TileSpecies.FindAsync(id);


            if (tileSpecies == null)
            {
                return NotFound();
            }

            return tileSpecies;
        }

        // GET: api/TileSpeciesApi/tile/5
        [HttpGet("tile/{id}")]
        public async Task<ActionResult<IEnumerable<TileSpecies>>> GetTileSpeciesOnTile(int id)
        {
            var sOnTile = await _context.TileSpecies.Where(s => s.TileId == 2).ToListAsync();

            if (sOnTile == null)
            {
                return NotFound();
            }

            return sOnTile;
        }

        // PUT: api/TileSpeciesApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTileSpecies(int id, TileSpecies tileSpecies)
        {
            if (id != tileSpecies.Id)
            {
                return BadRequest();
            }

            _context.Entry(tileSpecies).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TileSpeciesExists(id))
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

        // POST: api/TileSpeciesApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TileSpecies>> PostTileSpecies(TileSpecies tileSpecies)
        {
            _context.TileSpecies.Add(tileSpecies);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTileSpecies", new { id = tileSpecies.Id }, tileSpecies);
        }

        // DELETE: api/TileSpeciesApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TileSpecies>> DeleteTileSpecies(int id)
        {
            var tileSpecies = await _context.TileSpecies.FindAsync(id);
            if (tileSpecies == null)
            {
                return NotFound();
            }

            _context.TileSpecies.Remove(tileSpecies);
            await _context.SaveChangesAsync();

            return tileSpecies;
        }

        private bool TileSpeciesExists(int id)
        {
            return _context.TileSpecies.Any(e => e.Id == id);
        }
    }
}
