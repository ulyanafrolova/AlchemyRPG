using System.Collections.Generic;

namespace AlchemyRPG;

/// <summary>
/// Encapsulates data regarding an acoustic event generated within the game world.
/// Carries the origin coordinates and a pre-calculated map of tiles reached by the sound wave.
/// </summary>
public class NoiseData
{
    /// <summary>Gets the horizontal origin coordinate of the noise.</summary>
    public int SourceX { get; }

    /// <summary>Gets the vertical origin coordinate of the noise.</summary>
    public int SourceY { get; }

    /// <summary>
    /// Gets a dictionary mapping the coordinates of tiles reached by the sound 
    /// to their respective distance from the source.
    /// </summary>
    public Dictionary<(int x, int y), int> ReachedTiles { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseData"/> class.
    /// </summary>
    public NoiseData(int sourceX, int sourceY, Dictionary<(int x, int y), int> reachedTiles)
    {
        SourceX = sourceX;
        SourceY = sourceY;
        ReachedTiles = reachedTiles;
    }
}

/// <summary>
/// Represents an event triggered when an enemy entity is defeated.
/// Used primarily to notify remaining enemies of the same species to trigger behavioral reactions.
/// </summary>
public class EnemyDeathData
{
    /// <summary>Gets the species classification of the defeated enemy.</summary>
    public string Species { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnemyDeathData"/> class.
    /// </summary>
    public EnemyDeathData(string species)
    {
        Species = species;
    }
}

/// <summary>
/// Contains data regarding a specific enemy successfully perceiving an acoustic event.
/// Used for logging and potential state transitions.
/// </summary>
public class EnemyHeardNoiseData
{
    public string Species { get; }
    public int EnemyX { get; }
    public int EnemyY { get; }
    public int SourceX { get; }
    public int SourceY { get; }
    public int Distance { get; }

    public EnemyHeardNoiseData(string species, int ex, int ey, int sx, int sy, int dist)
    {
        Species = species;
        EnemyX = ex;
        EnemyY = ey;
        SourceX = sx;
        SourceY = sy;
        Distance = dist;
    }
}

/// <summary>
/// Contains data regarding a specific player successfully perceiving an acoustic event.
/// Used to route local UI feedback to the affected player's client.
/// </summary>
public class PlayerHeardNoiseData
{
    public string ListenerName { get; }
    public int ListenerX { get; }
    public int ListenerY { get; }
    public int SourceX { get; }
    public int SourceY { get; }
    public int Distance { get; }

    public PlayerHeardNoiseData(string listenerName, int lx, int ly, int sx, int sy, int dist)
    {
        ListenerName = listenerName;
        ListenerX = lx;
        ListenerY = ly;
        SourceX = sx;
        SourceY = sy;
        Distance = dist;
    }
}

/// <summary>
/// Encapsulates a system log message along with its categorization type.
/// Dispatched to the central logging service for file and diagnostic output.
/// </summary>
public class SystemLogData
{
    public LogType Type { get; }
    public string Message { get; }

    public SystemLogData(LogType type, string message)
    {
        Type = type;
        Message = message;
    }
}