namespace AlchemyRPG;

/// <summary>
/// Defines the contract for services capable of calculating sound propagation 
/// within the dungeon environment.
/// </summary>
public interface IAcousticService
{
    /// <summary>
    /// Calculates the propagation of a sound wave, returning a map of all reachable tiles 
    /// and their respective acoustic distance from the source.
    /// </summary>
    /// <param name="map">The map grid containing terrain data.</param>
    /// <param name="startX">The X coordinate of the sound origin.</param>
    /// <param name="startY">The Y coordinate of the sound origin.</param>
    /// <param name="maxRange">The maximum distance the sound can travel.</param>
    /// <returns>A dictionary mapping tile coordinates to their distance from the source.</returns>
    Dictionary<(int x, int y), int> CalculateAcousticDistances(Map map, int startX, int startY, int maxRange);
}

/// <summary>
/// Implements the <see cref="IAcousticService"/> using a Breadth-First Search (BFS) algorithm.
/// This system simulates sound propagation by expanding outward from the origin, 
/// stopping at walls and reaching the specified <paramref name="maxRange"/>.
/// </summary>
public class AcousticSystem : IAcousticService
{
    /// <summary>
    /// Calculates acoustic distances using a BFS approach.
    /// BFS is ideal here because it naturally finds the shortest path (distance) 
    /// from the source to any reachable tile.
    /// </summary>
    public Dictionary<(int x, int y), int> CalculateAcousticDistances(Map map, int startX, int startY, int maxRange)
    {
        var distances = new Dictionary<(int x, int y), int>();
        var queue = new Queue<(int x, int y, int dist)>();

        // Initialize the queue with the source position
        queue.Enqueue((startX, startY, 0));
        distances[(startX, startY)] = 0;

        // Directional vectors for cardinal movement (Up, Down, Left, Right)
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Stop propagation if the maximum sound range has been reached
            if (current.dist >= maxRange) continue;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                // Ensure neighbor is within map boundaries and is a traversable floor tile
                if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height && map.GetTileAt(nx, ny).IsWalkable)
                {
                    // If this tile hasn't been reached yet, record its distance and continue expansion
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
}