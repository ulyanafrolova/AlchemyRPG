using System;

namespace AlchemyRPG;

/// <summary>
/// Provides extension methods for the <see cref="Map"/> class to centralize shared logic.
/// This layer abstracts repetitive spawning tasks, ensuring consistent placement rules 
/// for items and entities across different map generation modifiers.
/// </summary>
public static class MapExtensions
{
    /// <summary>
    /// Searches for a random walkable floor tile and places the specified item at those coordinates.
    /// </summary>
    /// <param name="map">The map instance to populate.</param>
    /// <param name="rand">The random number generator for coordinate selection.</param>
    /// <param name="item">The item to be placed on the map.</param>
    public static void SpawnItemRandomly(this Map map, Random rand, IItem item)
    {
        var spawn = map.GetRandomWalkableTile(rand);
        if (spawn.HasValue)
            map.PlaceItemAt(spawn.Value.x, spawn.Value.y, item);
    }

    /// <summary>
    /// Searches for a random walkable floor tile and teleports the enemy to those coordinates,
    /// subsequently registering the enemy within the map's active entity list.
    /// </summary>
    /// <param name="map">The map instance to populate.</param>
    /// <param name="rand">The random number generator for coordinate selection.</param>
    /// <param name="enemy">The enemy entity to spawn.</param>
    public static void SpawnEnemyRandomly(this Map map, Random rand, Enemy enemy)
    {
        var spawn = map.GetRandomWalkableTile(rand);
        if (spawn.HasValue)
        {
            // Use Teleport to validate the spawn location via map rules
            enemy.Teleport(spawn.Value.x, spawn.Value.y, map);
            map.AddEnemy(enemy);
        }
    }
}