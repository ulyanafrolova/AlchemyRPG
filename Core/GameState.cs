namespace AlchemyRPG;

using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Represents the authoritative domain state of the entire game world.
/// This object serves as the core data container that the GameEngine operates on.
/// </summary>
public class GameState
{
    /// <summary>
    /// Gets the immutable configuration settings applied to the current session.
    /// </summary>
    public required GameConfig Config { get; init; }

    /// <summary>
    /// Gets the physical structure of the dungeon, including the terrain grid, entities, and items.
    /// </summary>
    public required Map Map { get; init; }

    /// <summary>
    /// Gets the dynamically generated tutorial text detailing lore and gameplay rules for the active theme.
    /// </summary>
    public string TutorialText { get; init; } = "";

    /// <summary>
    /// Gets the dynamically generated control instructions based on features present in the dungeon.
    /// </summary>
    public string ControlsText { get; init; } = "";

    /// <summary>
    /// Gets the service responsible for calculating sound propagation paths and distances.
    /// </summary>
    public required IAcousticService Acoustic { get; init; }

    /// <summary>
    /// The event bus for broadcasting raw noise generation coordinates.
    /// </summary>
    public required ISubject<NoiseData> NoiseEvents { get; init; }

    /// <summary>
    /// The event bus for broadcasting enemy elimination events.
    /// </summary>
    public required ISubject<EnemyDeathData> DeathEvents { get; init; }

    /// <summary>
    /// The event bus triggered when an enemy successfully perceives a noise.
    /// </summary>
    public required ISubject<EnemyHeardNoiseData> HeardNoiseEvents { get; init; }

    /// <summary>
    /// The event bus triggered when a player successfully perceives a noise.
    /// </summary>
    public required ISubject<PlayerHeardNoiseData> PlayerHeardNoiseEvents { get; init; }

    /// <summary>
    /// Gets the rolling memory buffer that stores the most recent game events for client UI consumption.
    /// </summary>
    public EventLog EventLog { get; } = new EventLog(5);

    /// <summary>
    /// The primary event bus used to route diagnostic, combat, and loot data to the file logger.
    /// </summary>
    public required ISubject<SystemLogData> SystemLogs { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current game session has ended.
    /// </summary>
    public bool IsGameOver { get; private set; } = false;

    /// <summary>
    /// Marks the global game state as concluded.
    /// </summary>
    public void SetGameOver() => IsGameOver = true;

    /// <summary>
    /// A thread-safe collection of all currently connected and active players in the session.
    /// </summary>
    public ConcurrentDictionary<int, Player> Players { get; } = new();

    /// <summary>
    /// Retrieves an enumeration of all active players participating in the session.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Player"/> objects.</returns>
    public IEnumerable<Player> GetAllActivePlayers()
    {
        return Players.Values;
    }
}