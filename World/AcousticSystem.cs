namespace AlchemyRPG;

public static class AcousticSystem
{
    public static Dictionary<(int x, int y), int> CalculateAcousticDistances(Map map, int startX, int startY, int maxRange)
    {
        var distances = new Dictionary<(int x, int y), int>();
        var queue = new Queue<(int x, int y, int dist)>();

        queue.Enqueue((startX, startY, 0));
        distances[(startX, startY)] = 0;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.dist >= maxRange) continue;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height && map.Grid[ny, nx] != Tiles.Wall)
                {
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