namespace AlchemyRPG;

/// <summary>
/// Implementation of the IDungeonBuilder interface.
/// aintain the map state and passes it along 
/// </summary>
public class DungeonBuilder : IDungeonBuilder
{
    private Map? _map;
    private int _width;
    private int _height;
    private readonly Random _rand = new();

    // Tutorial text
    private readonly List<string> _tutorialText = new() { "--- DUNGEON INSTRUCTIONS ---" };

    /// <summary>
    /// A collection that stores unique tutorial instructions.
    /// We use a HashSet to prevent duplicate instructions 
    /// (e.g., if multiple methods add the "[E] Pick Up" prompt).
    /// </summary>
    private readonly HashSet<string> _instructions = [$"[{Keybinds.MoveUp}{Keybinds.MoveDown}{Keybinds.MoveLeft}{Keybinds.MoveRight}] Move", $"[{Keybinds.Help}] Help", $"[{Keybinds.Journal}] Journal"];

    /// <summary>
    /// Initializes the foundational grid of the map as completely empty (walkable floor).
    /// </summary>
    public IDungeonBuilder CreateEmpty(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map.Grid[y, x] = Tiles.Floor;

        // Ensure the player cannot walk out of bounds
        AddBorders();
        return this;
    }

    /// <summary>
    /// Initializes the foundational grid of the map as completely solid 
    /// </summary>
    public IDungeonBuilder CreateFilled(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map.Grid[y, x] = Tiles.Wall;
        return this;
    }

    /// <summary>
    /// Executes the provided modifier against the current map state.
    /// </summary>
    public IDungeonBuilder ApplyModifier(IDungeonModifier modifier)
    {
        if (_map == null)
            throw new InvalidOperationException("CreateEmpty or CreateFilled must be called first!");

        // Inject the current state into the modifier so it can perform its specific task
        modifier.Apply(_map, _instructions, _tutorialText, _rand);

        return this;
    }

    /// <summary>
    /// Extracts the fully assembled map
    /// </summary>
    public Map GetMap()
    {
        EnsureInitialized();
        return _map!;
    }

    /// <summary>
    /// Generates the context-aware tutorial string.
    /// </summary>
    public string GetInstructions() => "Controls: " + string.Join(" | ", _instructions);

    /// <summary>
    /// Returns full game instruction.
    /// </summary>
    public string GetTutorialText() => string.Join("\n", _tutorialText);

    /// <summary>
    /// Guarantees that the builder sequence is valid
    /// </summary>
    private void EnsureInitialized()
    {
        if (_map == null)
            throw new InvalidOperationException("Initial building block (CreateEmpty or CreateFilled) must be called first!");
    }

    /// <summary>
    /// Prevents array out-of-bounds exceptions when the player attempts to move off the screen
    /// </summary>
    private void AddBorders()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (y == 0 || y == _height - 1 || x == 0 || x == _width - 1)
                    _map!.Grid[y, x] = Tiles.Wall;
    }
}
