namespace AlchemyRPG;

/// <summary>
/// Represents the physical game world. Handles the storage and retrieval of the terrain grid, 
/// management of static items, and tracking of dynamic entities (enemies) within the dungeon.
/// </summary>
public class Map
{
    /// <summary>Gets the width of the map grid.</summary>
    public int Width { get; }

    /// <summary>Gets the height of the map grid.</summary>
    public int Height { get; }

    /// <summary>
    /// Gets a revision counter that increments whenever the item state changes. 
    /// Used by the <see cref="StateMapper"/> to optimize serialization.
    /// </summary>
    public int Version { get; private set; } = 0;

    /// <summary> 
    /// A 2D array representing the terrain grid. 
    /// </summary>
    private readonly ITile[,] _grid;

    /// <summary>
    /// Retrieves the terrain type at the specified coordinate.
    /// This is the primary method for accessing grid data.
    /// </summary>
    public ITile GetTileAt(int x, int y) => _grid[y, x];

    /// <summary>
    /// Updates the terrain type at the specified coordinate.
    /// This is intended for use during dungeon generation processes.
    /// </summary>
    internal void SetTileAt(int x, int y, ITile tile) => _grid[y, x] = tile;
    /// <summary> 
    /// A collection storing all items placed on the map, indexed by their coordinate and item instance.
    /// </summary>
    private readonly List<(int X, int Y, IItem Item)> _items = [];

    private readonly List<Enemy> _enemies = new();

    /// <summary>Gets a read-only list of all active enemies on the map.</summary>
    public IReadOnlyList<Enemy> Enemies => _enemies.AsReadOnly();

    public void AddEnemy(Enemy enemy) => _enemies.Add(enemy);
    public void RemoveEnemy(Enemy enemy) => _enemies.Remove(enemy);

    /// <summary>
    /// Searches for an enemy at the specific coordinates.
    /// </summary>
    /// <returns>The enemy found, or <c>null</c> if the tile is unoccupied.</returns>
    public Enemy? GetEnemyAt(int x, int y)
    {
        return Enemies.FirstOrDefault(e => e.X == x && e.Y == y);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Map"/> class with the specified dimensions.
    /// </summary>
    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new ITile[height, width];
    }

    /// <summary>
    /// Places an item at the given coordinates and increments the map version.
    /// </summary>
    public void PlaceItemAt(int x, int y, IItem item)
    {
        _items.Add((x, y, item));
        Version++;
    }

    /// <summary>
    /// Removes an item from the map and increments the map version.
    /// </summary>
    public void RemoveItem(int x, int y, IItem item)
    {
        _items.Remove((x, y, item));
        Version++;
    }

    /// <summary>
    /// Retrieves all items currently located at the specified exact coordinates.
    /// </summary>
    /// <returns>A list of items at the position, or an empty list if none exist.</returns>
    public List<IItem> GetItemsAt(int x, int y)
    {
        var result = new List<IItem>();
        foreach (var itemData in _items)
        {
            if (itemData.X == x && itemData.Y == y)
                result.Add(itemData.Item);
        }
        return result;
    }

    /// <summary>
    /// Returns a list of all items placed throughout the entire map.
    /// </summary>
    public IReadOnlyList<(int X, int Y, IItem Item)> GetAllItems()
    {
        return _items.ToList();
    }

    /// <summary>
    /// Determines whether a coordinate is safe for navigation.
    /// A tile is walkable if it is within bounds, not a wall, and not occupied by an enemy.
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        if (!GetTileAt(x, y).IsWalkable) return false;
        if (GetEnemyAt(x, y) != null)
            return false;

        return true;
    }

    /// <summary>
    /// Searches for a random tile that is marked as walkable.
    /// Useful for spawning entities or placing items.
    /// </summary>
    /// <returns>The coordinates of a safe tile, or <c>null</c> if no such tile is found within the attempt limit.</returns>
    public (int x, int y)? GetRandomWalkableTile(Random rand)
    {
        int spawnX, spawnY;
        int attempts = 0;
        const int MaxAttempts = 100;

        do
        {
            spawnX = rand.Next(1, Width - 1);
            spawnY = rand.Next(1, Height - 1);
            attempts++;
            if (attempts >= MaxAttempts) return null;
        }
        while (!IsWalkable(spawnX, spawnY));

        return (spawnX, spawnY);
    }
}