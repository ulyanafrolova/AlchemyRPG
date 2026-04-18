namespace AlchemyRPG;

/// <summary>
/// A utility class that generates a random maze using the Depth-First Search (Recursive Backtracker) algorithm.
/// </summary>
public static class Labyrinth
{
    private static readonly Random Rand = new();

    /// <summary>
    /// Generates a 2D grid representing a maze.
    /// </summary>
    /// <param name="width">The width of the map.</param>
    /// <param name="height">The height of the map.</param>
    /// <returns>A 2D char array where '█' is a wall and ' ' is a walkable path.</returns>
    public static char[,] Generate(int width, int height, int randomHoles = 30)
    {
        char[,] grid = new char[height, width];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid[y, x] = Tiles.Wall;

        Stack<(int X, int Y)> stack = new();

        int startX = 1;
        int startY = 1;

        grid[startY, startX] = Tiles.Floor;
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var neighbors = GetUnvisitedNeighbors(current.X, current.Y, grid, width, height);

            if (neighbors.Count > 0)
            {
                stack.Push(current);
                var next = neighbors[Rand.Next(neighbors.Count)];
                int wallX = current.X + (next.X - current.X) / 2;
                int wallY = current.Y + (next.Y - current.Y) / 2;

                grid[wallY, wallX] = Tiles.Floor; 
                grid[next.Y, next.X] = Tiles.Floor; 

                stack.Push(next);
            }
        }

        for (int x = 1; x < width - 1; x++)
        {
            if (grid[height - 3, x] == Tiles.Floor)
            {
                grid[height - 2, x] = Tiles.Floor; 
            }
        }
        for (int y = 1; y < height - 1; y++)
        {
            if (grid[y, width - 3] == Tiles.Floor)
            {
                grid[y, width - 2] = Tiles.Floor; 
            }
        }

        for (int i = 0; i < randomHoles; i++)
        {
            int rx = Rand.Next(1, width - 1);
            int ry = Rand.Next(1, height - 1);
            grid[ry, rx] = Tiles.Floor;
        }

        return grid;
    }

    /// <summary>
    /// Finds valid neighbors 2 steps away that are still walls.
    /// </summary>
    private static List<(int X, int Y)> GetUnvisitedNeighbors(int x, int y, char[,] grid, int width, int height)
    {
        var neighbors = new List<(int X, int Y)>();

        if (x >= 3 && grid[y, x - 2] == Tiles.Wall) neighbors.Add((x - 2, y)); 
        if (x <= width - 4 && grid[y, x + 2] == Tiles.Wall) neighbors.Add((x + 2, y)); 
        if (y >= 3 && grid[y - 2, x] == Tiles.Wall) neighbors.Add((x, y - 2)); 
        if (y <= height - 4 && grid[y + 2, x] == Tiles.Wall) neighbors.Add((x, y + 2)); 

        return neighbors;
    }
}