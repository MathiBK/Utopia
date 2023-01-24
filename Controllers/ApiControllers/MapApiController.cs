using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using utopia.Data;
using utopia.Models;

namespace utopia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MapApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MapApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tile>>> GetTiles()
        {
            return await _context.Tiles.ToListAsync();
        }

        // GET: api/MapApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tile>> GetTile(int id)
        {
            var tile = await _context.Tiles.FindAsync(id);

            if (tile == null)
            {
                return NotFound();
            }

            return tile;
        }

        // PUT: api/MapApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTile(int id, Tile tile)
        {
            if (id != tile.Id)
            {
                return BadRequest();
            }

            _context.Entry(tile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TileExists(id))
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

        // POST: api/MapApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Tile>> PostTile(Tile tile)
        {
            _context.Tiles.Add(tile);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTile", new { id = tile.Id }, tile);
            return CreatedAtAction(nameof(GetTile), new { id = tile.Id }, tile);
        }

        // DELETE: api/MapApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tile>> DeleteTile(int id)
        {
            var tile = await _context.Tiles.FindAsync(id);
            if (tile == null)
            {
                return NotFound();
            }

            _context.Tiles.Remove(tile);
            await _context.SaveChangesAsync();

            return tile;
        }

        private bool TileExists(int id)
        {
            return _context.Tiles.Any(e => e.Id == id);
        }
    }
}
