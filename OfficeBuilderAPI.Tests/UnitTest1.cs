using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeBuilderAPI;

namespace OfficeBuilderAPI.Tests;

public class IntegrationTests
{
    private AppDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("insertSQLConnectionString")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Test_Create_And_Load_User()
    {
        using var db = GetDb();

        var user = new User
        {
            Username = "integrationuser",
            PasswordHash = new byte[] { 1, 2, 3 },
            PasswordSalt = new byte[] { 4, 5, 6 }
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var saved = await db.Users.FirstOrDefaultAsync(u => u.Username == "integrationuser");

        Assert.NotNull(saved);
        Assert.Equal("integrationuser", saved.Username);
    }

    [Fact]
    public async Task Test_World_Creation_And_Query()
    {
        using var db = GetDb();

        var user = db.Users.FirstOrDefault(u => u.Username == "integrationuser");
        Assert.NotNull(user);

        var world = new World
        {
            UserId = user.Id,
            WorldName = "IntegrationWorld"
        };

        db.Worlds.Add(world);
        await db.SaveChangesAsync();

        var found = await db.Worlds.FirstOrDefaultAsync(w => w.WorldName == "IntegrationWorld");
        Assert.NotNull(found);
        Assert.Equal(user.Id, found.UserId);
    }

    [Fact]
    public async Task Test_TileData_Storage()
    {
        using var db = GetDb();

        var world = await db.Worlds.FirstOrDefaultAsync(w => w.WorldName == "IntegrationWorld");
        Assert.NotNull(world);

        var tile = new TileData
        {
            TileType = "BasicTile1",
            X = 0,
            Y = 0,
            WorldId = world.Id
        };

        db.TileData.Add(tile);
        await db.SaveChangesAsync();

        var tiles = await db.TileData.Where(t => t.WorldId == world.Id).ToListAsync();

        Assert.Contains(tiles, t => t.TileType == "BasicTile1" && t.X == 0 && t.Y == 0);
    }
}