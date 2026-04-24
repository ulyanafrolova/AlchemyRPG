using System;

namespace AlchemyRPG;

/// <summary>
/// Implements the <see cref="IEventListener"/> interface to participate in the Event Bus (Observer pattern).
/// Inherits from <see cref="Entity"/> to allow integration with the existing map grid system, 
/// meaning enemies occupy tiles just like items, but trigger combat interactions.
/// </summary>
public class Enemy : Entity, INoiseListener, IEnemyDeathListener
{
    /// <summary>
    /// Gets the biological or magical classification of the enemy (e.g., "Goblin", "Skeleton").
    /// Used for pack-behavior and event filtering.
    /// </summary>
    public string Species { get; }

    /// <summary>
    /// Gets the base attack power of the enemy. 
    /// Includes a private setter to allow dynamic stat changes based on kinsman deaths.
    /// </summary>
    public int AttackDamage { get; private set; }

    /// <summary>
    /// Gets the defensive armor rating of the enemy.
    /// </summary>
    public int Armor { get; }

    private readonly IKinsmanDeathBehavior _deathBehavior;

    private readonly EventManager _events;

    /// <summary>
    /// Initializes a new instance of the <see cref="Enemy"/> class and subscribes to global events.
    /// </summary>
    public Enemy(string name, string species, int health, int attackDamage, int armor, EventManager events, IKinsmanDeathBehavior deathBehavior = null)
   : base(name, Tiles.Enemy, health)
    {
        Species = species;
        AttackDamage = attackDamage;
        Armor = armor;
        _events = events;
        _deathBehavior = deathBehavior ?? new NeutralBehavior();

        _events.Subscribe<NoiseData>(this);
        _events.Subscribe<EnemyDeathData>(this);
    }

    public void ModifyAttackDamage(int delta)
    {
        AttackDamage = Math.Max(0, AttackDamage + delta);
    }
    private void ReactToKinsmanDeath()
    {
        _deathBehavior.React(this);
    }

    public void OnEvent(NoiseData noise)
    {
        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distanceToSource))
        {
            GameLogger.Instance.Log(LogType.System,
                $"[{Species} at {X},{Y}] Heard a noise from {noise.SourceX},{noise.SourceY} (Distance: {distanceToSource} steps).");
        }
    }

    public void OnEvent(EnemyDeathData deathInfo)
    {
        if (deathInfo.Species == this.Species)
        {
            ReactToKinsmanDeath();
        }
    }

    /// <summary>
    /// Safely prepares the enemy for removal from the game world.
    /// Must be called by the Combat System when the enemy's health reaches zero.
    /// </summary>
    public void TriggerDeathProcessing()
    {
        _events.Notify(new EnemyDeathData(this.Species));
        _events.Unsubscribe<NoiseData>(this);
        _events.Unsubscribe<EnemyDeathData>(this);
    }

    /// <summary>
    /// Performs a random movement to an adjacent valid tile.
    /// This simulates active wandering through the dungeon corridors.
    /// </summary>
    /// <param name="map">The current game map to check for walls and obstacles.</param>
    /// <param name="rand">The random number generator.</param>
    public void MoveRandomly(Map map, Random rand)
    {
        // 4 possible directions: Up, Down, Left, Right
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        // Pick a random direction
        int direction = rand.Next(4);
        int newX = X + dx[direction];
        int newY = Y + dy[direction];

        // Ensure the new tile is a floor (not a wall)
        // Also ensure the enemy doesn't step out of bounds
        if (newX >= 0 && newX < map.Width && newY >= 0 && newY < map.Height)
        {
            if (map.IsWalkable(newX, newY))
            {
                // Update the enemy's coordinates
                X = newX;
                Y = newY;
            }
        }
    }
}