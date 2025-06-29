using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorldsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorldsController(AppDbContext context)
    {
         _context = context;
    }

    // Create a new world.
    [HttpPost("create")]
    public async Task<IActionResult> CreateWorld(CreateWorldDto request)
    {
         int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

         // Validate world name length.
         if (string.IsNullOrEmpty(request.WorldName) || request.WorldName.Length < 1 || request.WorldName.Length > 25)
             return BadRequest("World name must be between 1 and 25 characters.");

         // Check if the user already has 5 worlds.
         int worldCount = await _context.Worlds.CountAsync(w => w.UserId == userId);
         if (worldCount >= 5)
             return BadRequest("Cannot create more than 5 worlds.");

         // Check for duplicate world name for this user.
         bool exists = await _context.Worlds.AnyAsync(w => w.UserId == userId && w.WorldName == request.WorldName);
         if (exists)
             return BadRequest("World name already exists.");

         var world = new World
         {
             WorldName = request.WorldName,
             UserId = userId
         };

         _context.Worlds.Add(world);
         await _context.SaveChangesAsync();
         return Ok("World created successfully.");
    }

    // Get an overview of the user's worlds.
    [HttpGet("overview")]
    public async Task<IActionResult> GetWorlds()
    {
         int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
         var worlds = await _context.Worlds.Where(w => w.UserId == userId).ToListAsync();
         return Ok(worlds);
    }

    // Get a specific world.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorld(int id)
    {
         int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
         var world = await _context.Worlds.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
         if (world == null)
             return NotFound("World not found.");
         return Ok(world);
    }

    // Delete a world (and associated 2D objects if applicable).
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorld(int id)
    {
         int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
         var world = await _context.Worlds.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
         if (world == null)
             return NotFound("World not found.");

         // TODO: Also delete associated 2D objects if they exist.
         _context.Worlds.Remove(world);
         await _context.SaveChangesAsync();
         return Ok("World deleted successfully.");
    }
}