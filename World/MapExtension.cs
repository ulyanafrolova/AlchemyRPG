namespace AlchemyRPG;

/// <summary>
/// Provides extension methods for the Map class to centralize shared logic.
/// This eliminates code duplication (DRY principle) across different modifiers
/// that need to spawn items or entities randomly on valid floor tiles.
/// </summary>
public static class MapExtensions
{
    /// <summary>
    /// Finds a random unoccupied floor tile and places the specified item there.
    /// </summary>
    /// <param name="map">The map instance being extended.</param>
    /// <param name="rand">The random number generator.</param>
    /// <param name="item">The item or entity to spawn.</param>
    public static void SpawnItemRandomly(this Map map, Random rand, IItem item)
    {
        int x, y;
        do
        {
            x = rand.Next(1, map.Width - 1);
            y = rand.Next(1, map.Height - 1);
        } while (map.Grid[y, x] != Tiles.Floor || map.GetItemsAt(x, y).Count > 0);

        map.AddItem(x, y, item);
    }
}