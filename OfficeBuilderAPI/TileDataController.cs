using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


[ApiController]
[Route("api/worlds/{worldId}/tiles")]
[Authorize]
public class TileDataController : ControllerBase
{
    public class TileWrapper
    {
        public List<TileDto> Tiles { get; set; } = new();
    }

    private readonly AppDbContext _context;

    public TileDataController(AppDbContext context)
    {
        _context = context;
    }

    // GET all tiles for a world
    [HttpGet]
    public async Task<IActionResult> GetTiles(int worldId)
    {
        var tiles = await _context.TileData
            .Where(t => t.WorldId == worldId)
            .Select(t => new TileDto
            {
                TileType = t.TileType,
                X = t.X,
                Y = t.Y
            })
            .ToListAsync();

        return Ok(tiles);
    }

    // PUT (overwrite) tiles for a world
    [HttpPut]
    public async Task<IActionResult> SaveTiles(int worldId, [FromBody] TileWrapper data)
    {
        var tileDtos = data.Tiles;
        var world = await _context.Worlds.Include(w => w.Tiles).FirstOrDefaultAsync(w => w.Id == worldId);
        if (world == null) return NotFound("World not found.");

        // Remove existing tiles
        _context.TileData.RemoveRange(world.Tiles);

        // Add new tiles
        foreach (var tile in tileDtos)
        {
            world.Tiles.Add(new TileData
            {
                TileType = tile.TileType,
                X = tile.X,
                Y = tile.Y,
                WorldId = worldId
            });
        }

        await _context.SaveChangesAsync();
        return Ok("Tiles saved.");
    }
}
