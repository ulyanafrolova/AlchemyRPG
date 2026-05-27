namespace AlchemyRPG;

/// <summary>
/// A modifier strategy that carves procedural pathways into the dungeon grid.
/// It utilizes the <see cref="Labyrinth"/> generator to ensure the dungeon is traversable 
/// by connecting isolated open areas.
/// </summary>
public class CorridorsModifier : IDungeonModifier
{
    /// <summary>
    /// Applies the maze generation algorithm to the map.
    /// </summary>
    /// <param name="map">The map grid to be modified.</param>
    /// <param name="controls">The UI control instruction registry.</param>
    /// <param name="tutorialText">The list of narrative tutorial lines.</param>
    /// <param name="rand">The shared random number generator for procedural variance.</param>
    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- You must navigate through a twisting maze of corridors.");
        
        var maze = Labyrinth.Generate(map.Width, map.Height);
        
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (maze[y, x] == TerrainType.Floor) 
                    map.SetTileAt(x, y, TerrainType.Floor);
            }
        }
    }
}

/// <summary>
/// A modifier strategy that carves multiple rectangular open spaces into the map.
/// These rooms act as points of interest and combat arenas.
/// </summary>
public class RoomsModifier : IDungeonModifier
{
    private readonly int _numberOfRooms;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomsModifier"/> class.
    /// </summary>
    /// <param name="count">The number of rectangular rooms to attempt to spawn.</param>
    public RoomsModifier(int count) => _numberOfRooms = count;

    /// <summary>
    /// Randomly places rectangular rooms within the map boundaries.
    /// </summary>
    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- Explore various hidden rooms scattered across the area.");
        
        for (int i = 0; i < _numberOfRooms; i++)
        {
            int w = rand.Next(3, 8), h = rand.Next(3, 6);
            int x = rand.Next(1, map.Width - w - 1), y = rand.Next(1, map.Height - h - 1);
            
            for (int ry = y; ry < y + h; ry++)
            {
                for (int rx = x; rx < x + w; rx++)
                {
                    map.SetTileAt(rx, ry, TerrainType.Floor);
                }
            }
        }
    }
}

/// <summary>
/// A modifier strategy that carves a large rectangular room in the geometric center of the dungeon.
/// Useful for spawning primary objectives or "Boss" arenas.
/// </summary>
public class CentralRoomModifier : IDungeonModifier
{
    private readonly int _roomWidth;
    private readonly int _roomHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="CentralRoomModifier"/> class.
    /// </summary>
    /// <param name="width">The width of the central room.</param>
    /// <param name="height">The height of the central room.</param>
    public CentralRoomModifier(int width, int height)
    {
        _roomWidth = width;
        _roomHeight = height;
    }

    /// <summary>
    /// Calculates the center of the map and overwrites tiles to create a central hall.
    /// </summary>
    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- A massive central hall lies in the middle of the dungeon.");
        
        int startX = (map.Width - _roomWidth) / 2;
        int startY = (map.Height - _roomHeight) / 2;
        
        for (int y = startY; y < startY + _roomHeight; y++)
        {
            for (int x = startX; x < startX + _roomWidth; x++)
            {
                map.SetTileAt(x, y, TerrainType.Floor);
            }
        }
    }
}