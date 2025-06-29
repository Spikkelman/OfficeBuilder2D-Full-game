using System.ComponentModel.DataAnnotations;

public class World
{
    public int Id { get; set; }
    
    [Required, MaxLength(25)]
    public string WorldName { get; set; }
    
    // Foreign key to the owning user
    public int UserId { get; set; }
    public User User { get; set; }
    public List<TileData> Tiles { get; set; } = new();
}
