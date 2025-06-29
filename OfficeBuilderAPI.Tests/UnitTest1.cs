using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeBuilderAPI;

namespace OfficeBuilderAPI.Tests;

public class TileDbContextTests
{
    private AppDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SaveTilesDirectlyToDb_ShouldStoreTiles()
    {
        var db = CreateDb("TestDb1");

        var user = new User { Username = "user1", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var world = new World { WorldName = "WorldOne", UserId = user.Id };
        db.Worlds.Add(world);
        await db.SaveChangesAsync();

        db.TileData.AddRange(new List<TileData>
        {
            new TileData { TileType = "BasicTile1", X = 0, Y = 0, WorldId = world.Id },
            new TileData { TileType = "BasicTile2", X = 1, Y = 1, WorldId = world.Id }
        });

        await db.SaveChangesAsync();

        var tiles = await db.TileData.Where(t => t.WorldId == world.Id).ToListAsync();
        Assert.Equal(2, tiles.Count);
    }

    [Fact]
    public async Task LoadTiles_ShouldReturnCorrectTileData()
    {
        var db = CreateDb("TestDb2");

        var user = new User { Username = "user2", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var world = new World { WorldName = "WorldTwo", UserId = user.Id };
        db.Worlds.Add(world);
        await db.SaveChangesAsync();

        db.TileData.Add(new TileData { TileType = "BasicTile3", X = -2, Y = 5, WorldId = world.Id });
        await db.SaveChangesAsync();

        var tile = await db.TileData.FirstOrDefaultAsync(t => t.WorldId == world.Id);
        Assert.NotNull(tile);
        Assert.Equal("BasicTile3", tile.TileType);
        Assert.Equal(-2, tile.X);
        Assert.Equal(5, tile.Y);
    }

    [Fact]
    public async Task NoTilesSaved_ShouldReturnEmptyList()
    {
        var db = CreateDb("TestDb3");

        var user = new User { Username = "user3", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var world = new World { WorldName = "WorldEmpty", UserId = user.Id };
        db.Worlds.Add(world);
        await db.SaveChangesAsync();

        var tiles = await db.TileData.Where(t => t.WorldId == world.Id).ToListAsync();
        Assert.Empty(tiles);
    }
}
