public class TileData
{
    public int Id { get; set; }
    public string TileType { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }

    public int WorldId { get; set; }
    public World? World { get; set; }
}
