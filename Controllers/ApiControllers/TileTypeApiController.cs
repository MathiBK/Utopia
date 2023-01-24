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
    public class TileTypeApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TileTypeApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TileTypeApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TileType>>> GetTileTypes()
        {
            return await _context.TileTypes.ToListAsync();
        }

        // GET: api/TileTypeApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TileType>> GetTileType(int id)
        {
            var tileType = await _context.TileTypes.FindAsync(id);

            if (tileType == null)
            {
                return NotFound();
            }

            return tileType;
        }

        // PUT: api/TileTypeApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTileType(int id, TileType tileType)
        {
            if (id != tileType.Id)
            {
                return BadRequest();
            }

            _context.Entry(tileType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TileTypeExists(id))
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

        // POST: api/TileTypeApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TileType>> PostTileType(TileType tileType)
        {
            _context.TileTypes.Add(tileType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTileType", new { id = tileType.Id }, tileType);
        }

        // DELETE: api/TileTypeApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TileType>> DeleteTileType(int id)
        {
            var tileType = await _context.TileTypes.FindAsync(id);
            if (tileType == null)
            {
                return NotFound();
            }

            _context.TileTypes.Remove(tileType);
            await _context.SaveChangesAsync();

            return tileType;
        }

        private bool TileTypeExists(int id)
        {
            return _context.TileTypes.Any(e => e.Id == id);
        }
    }
}
