namespace AlchemyRPG;

/// <summary>
/// A concrete implementation of the <see cref="IDungeonBuilder"/> interface.
/// This class manages the construction process of a <see cref="Map"/> by maintaining its state, 
/// applying modular modifications, and tracking contextual UI instructions for the player.
/// </summary>
public class DungeonBuilder : IDungeonBuilder
{
    private Map? _map;
    private int _width;
    private int _height;
    private readonly Random _rand = new();

    /// <summary>
    /// A running list of tutorial lines used to assemble the player's "How to Play" manual.
    /// </summary>
    private readonly List<string> _tutorialText = new() { "--- DUNGEON INSTRUCTIONS ---" };

    /// <summary>
    /// A set of unique control scheme strings. We use a <see cref="HashSet{T}"/> 
    /// to prevent duplicate instructions if multiple modifiers attempt to register the same controls.
    /// </summary>
    private readonly HashSet<string> _instructions = [
        $"[{Keybinds.MoveUp}{Keybinds.MoveDown}{Keybinds.MoveLeft}{Keybinds.MoveRight}] Move",
        $"[{Keybinds.Help}] Help",
        $"[{Keybinds.Journal}] Journal"
    ];

    /// <summary>
    /// Initializes a map grid composed entirely of walkable floor tiles.
    /// </summary>
    public IDungeonBuilder CreateEmpty(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map.SetTileAt(x, y, new FloorTile());

        AddBorders();
        return this;
    }

    /// <summary>
    /// Initializes a map grid where every tile is a solid wall.
    /// Used as a foundational canvas for modifiers that carve out rooms and corridors.
    /// </summary>
    public IDungeonBuilder CreateFilled(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _map.SetTileAt(x, y, new WallTile());

        return this;
    }

    /// <summary>
    /// Applies a specific generation algorithm (modifier) to the current map state.
    /// </summary>
    /// <param name="modifier">The strategy or algorithm to modify the map structure or population.</param>
    /// <exception cref="InvalidOperationException">Thrown if called before <see cref="CreateEmpty"/> or <see cref="CreateFilled"/>.</exception>
    public IDungeonBuilder ApplyModifier(IDungeonModifier modifier)
    {
        if (_map == null)
            throw new InvalidOperationException("Initial building block (CreateEmpty or CreateFilled) must be called first!");

        modifier.Apply(_map, _instructions, _tutorialText, _rand);
        return this;
    }

    /// <summary>
    /// Returns the completed map after the construction process is finalized.
    /// </summary>
    public Map GetMap()
    {
        EnsureInitialized();
        return _map!;
    }

    /// <summary>
    /// Assembles all registered controls into a single displayable instruction string.
    /// </summary>
    public string GetInstructions() => "Controls: " + string.Join(" | ", _instructions);

    /// <summary>
    /// Assembles the full tutorial manual from all registered narrative snippets.
    /// </summary>
    public string GetTutorialText() => string.Join("\n", _tutorialText);

    /// <summary>
    /// Verifies that the builder has been properly initialized with a map.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_map == null)
            throw new InvalidOperationException("Initial building block (CreateEmpty or CreateFilled) must be called first!");
    }

    /// <summary>
    /// Enforces map boundaries by setting the outer perimeter of the grid to floor tiles 
    /// (or walls, depending on the desired boundary collision behavior).
    /// </summary>
    private void AddBorders()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (y == 0 || y == _height - 1 || x == 0 || x == _width - 1)
                    _map!.SetTileAt(x, y, new FloorTile());
    }
}