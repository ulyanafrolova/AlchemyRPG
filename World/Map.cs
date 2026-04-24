namespace AlchemyRPG;

/// <summary>
/// Represents the physical game world. Handles the generation of the map grid, 
/// random item placement, collision detection, and rendering of the entire screen.
/// </summary>
public class Map
{
    public int Width { get; }
    public int Height { get; }

    /// <summary> 
    /// A 2D array representing the terrain ('█' for walls, ' ' for empty floor). 
    /// </summary>
    public char[,] Grid { get; }

    /// <summary> 
    /// A collection storing all items currently placed on the map along with their coordinates. 
    /// </summary>
    private readonly List<(int X, int Y, IItem Item)> _items = [];

    public List<Enemy> Enemies { get; } = new List<Enemy>();

    public Enemy? GetEnemyAt(int x, int y)
    {
        return Enemies.FirstOrDefault(e => e.X == x && e.Y == y);
    }

    /// <summary>
    /// Initializes a new instance of the Map class.
    /// Calls the external Labyrinth generator and populates the map with initial items.
    /// </summary>
    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new char[height, width];
    }

    /// <summary>
    /// Explicitly places an item at the exact given coordinates. Commonly used when a player drops an item.
    /// </summary>
    public void PlaceItemAt(int x, int y, IItem item) => _items.Add((x, y, item));

    /// <summary> 
    /// Adds an item to the map's active tracking list. 
    /// </summary>
    public void AddItem(int x, int y, IItem item) => _items.Add((x, y, item));

    /// <summary> 
    /// Removes an item from the map, typically called when the player picks it up. 
    /// </summary>
    public void RemoveItem(int x, int y, IItem item) => _items.Remove((x, y, item));

    /// <summary>
    /// Retrieves all items currently located at the specified exact coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns>A list of items at the given position, or an empty list if none exist.</returns>
    public List<IItem> GetItemsAt(int x, int y) => [.. _items.Where(i => i.X == x && i.Y == y).Select(i => i.Item)];

    /// <summary>
    /// Determines whether a specific coordinate is safe for the player to walk on.
    /// Checks grid boundaries to prevent IndexOutOfRange exceptions and ensures the tile is not a wall.
    /// </summary>
    /// <param name="x">The target X coordinate.</param>
    /// <param name="y">The target Y coordinate.</param>
    /// <returns><c>true</c> if the coordinate is within bounds and walkable; otherwise, <c>false</c>.</returns>
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || Grid[y, x] == Tiles.Wall)
            return false;

        if (GetEnemyAt(x, y) != null)
            return false;

        return true;
    }

    private char GetSymbolToDraw(GameState state, int x, int y)
    {
        var items = GetItemsAt(x, y);
        var renderLayers = new Func<char?>[]
        {
            () => (state.Player.X == x && state.Player.Y == y) ? state.Player.Symbol : null,
            () => GetEnemyAt(x, y)?.Symbol,
            () => items.Count > 1 ? Tiles.SeveralItems : null,
            () => items.Count == 1 ? items.First().Symbol : null,
            () => Grid[y, x]
        };
        return renderLayers.Select(layer => layer()).FirstOrDefault(s => s != null) ?? ' ';
    }
    /// <summary>
    /// Renders the complete game screen frame by frame.
    /// Draws the map grid, player position, items, and the side/bottom panels.
    /// </summary>
    /// <param name="state">The current game state, providing access to player position and logs.</param>
    public void Draw(GameState state)
    {
        var p = state.Player;

        for (int y = 0; y < Height; y++)
        {
            string mapLine = "";

            for (int x = 0; x < Width; x++)
            {
                mapLine += GetSymbolToDraw(state, x, y);
            }

            string statsLine = GetStatsLine(y, p);
            Console.WriteLine($"{mapLine}   {statsLine}".PadRight(100));
        }

        Console.WriteLine("".PadRight(Config.PaddingRight));

        var standingOn = GetItemsAt(p.X, p.Y);
        string groundInfo = standingOn.Count switch
        {
            > 1 => $"Ground: {standingOn.Count} items ({Tiles.SeveralItems}). Top: {standingOn.First().Name} (Press [{Keybinds.PickUp}])",
            1 => $"Ground: {standingOn.First().Name} (Press [{Keybinds.PickUp}])",
            _ => "Ground: Empty"
        };
        Console.WriteLine(groundInfo.PadRight(Config.PaddingRight));
        Console.WriteLine($"Log: {state.Log} {p.LogMessage}".PadRight(Config.PaddingRight));
        p.LogMessage = "";
        Console.WriteLine(state.Instructions.PadRight(Config.PaddingRight));


        Console.WriteLine("--- RECENT EVENTS ---".PadRight(Config.PaddingRight));
        var recentLogs = GameLogger.Instance.GetRecentLogs(3);
        if (recentLogs.Count == 0) Console.WriteLine("".PadRight(Config.PaddingRight));
        foreach (var log in recentLogs)
        {
            Console.WriteLine(log.ToString().PadRight(Config.PaddingRight));
        }
    }

    /// <summary>
    /// Generates the text for the right-side UI panel based on the current vertical rendering row.
    /// </summary>
    /// <param name="y">The current row (Y-coordinate) being drawn on the screen.</param>
    /// <param name="p">The player object containing the stats to display.</param>
    /// <returns>A formatted string representing a specific line of user interface.</returns>
    private static string GetStatsLine(int y, Player p)
    {
        return y switch
        {
            0 => "--- STATISTICS ---",
            1 => $"HP: {p.Health} | STR: {p.Strength} | DEX: {p.Dexterity}",
            2 => $"WIS: {p.Wisdom} | AGR: {p.Aggression} | LUCK: {p.TotalLuck}",
            3 => $"Gold: {p.Gold} | Coins: {p.Coins}",
            5 => "--- HANDS ---",
            6 => $"Left: {(p.LeftHand?.Name ?? "Empty")}",
            7 => $"Right: {(p.RightHand?.Name ?? "Empty")}",
            9 => "--- INVENTORY ---",
            _ => (y >= 10 && y < 10 + p.Backpack.Count) ? $"[{y - 10}] {p.Backpack[y - 10].Name}" : ""
        };
    }

    public Dictionary<(int x, int y), int> CalculateAcousticDistances(int startX, int startY, int maxRange)
    {
        var distances = new Dictionary<(int x, int y), int>();

        var queue = new Queue<(int x, int y, int dist)>();

        queue.Enqueue((startX, startY, 0));
        distances[(startX, startY)] = 0;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.dist >= maxRange) continue;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && IsWalkable(nx, ny))
                {
                    if (!distances.ContainsKey((nx, ny)))
                    {
                        distances[(nx, ny)] = current.dist + 1;
                        queue.Enqueue((nx, ny, current.dist + 1));
                    }
                }
            }
        }
        return distances;
    }

    public (int x, int y) GetRandomWalkableTile(Random rand)
    {
        int spawnX, spawnY;
        do
        {
            spawnX = rand.Next(1, Width - 1);
            spawnY = rand.Next(1, Height - 1);
        } 
        while (!IsWalkable(spawnX, spawnY)); 
        return (spawnX, spawnY);
    }
}