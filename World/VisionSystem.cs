namespace AlchemyRPG;

/// <summary>
/// Defines the contract for a vision system that determines which players are visible to an enemy based on line of sight and obstacles.
/// </summary>
public interface IVisionService
{
    List<Player> GetVisiblePlayers(Enemy enemy, GameState state, int sightRange = 8);
    Player? GetVisiblePlayer(Enemy enemy, GameState state, int sightRange = 8);
    bool IsSurroundedByPlayers(Enemy enemy, GameState state, int requiredCount = 4);
}

/// <summary>
/// Implements the IVisionService to provide line-of-sight calculations for enemies to detect players in the game world.
/// </summary>
public class VisionSystem : IVisionService
{
    private static readonly int[] Dx = { 0, 0, -1, 1 };
    private static readonly int[] Dy = { -1, 1, 0, 0 };

    public List<Player> GetVisiblePlayers(Enemy enemy, GameState state, int sightRange = 8)
    {
        var visiblePlayers = new List<Player>();
        for (int dir = 0; dir < 4; dir++)
        {
            var player = CastRay(enemy.X, enemy.Y, Dx[dir], Dy[dir], sightRange, state, out _);
            if (player != null && !visiblePlayers.Contains(player))
            {
                visiblePlayers.Add(player);
            }
        }
        return visiblePlayers;
    }

    public Player? GetVisiblePlayer(Enemy enemy, GameState state, int sightRange = 8)
    {
        Player? closest = null;
        int minDistance = int.MaxValue;

        for (int dir = 0; dir < 4; dir++)
        {
            var player = CastRay(enemy.X, enemy.Y, Dx[dir], Dy[dir], sightRange, state, out int distance);
            if (player != null && distance < minDistance)
            {
                minDistance = distance;
                closest = player;
            }
        }
        return closest;
    }

    private Player? CastRay(int startX, int startY, int dx, int dy, int maxRange, GameState state, out int distance)
    {
        distance = int.MaxValue;
        for (int step = 1; step <= maxRange; step++)
        {
            int checkX = startX + (dx * step);
            int checkY = startY + (dy * step);

            if (checkX < 0 || checkX >= state.Map.Width || checkY < 0 || checkY >= state.Map.Height)
                break;

            if (!state.Map.GetTileAt(checkX, checkY).IsWalkable ||
               (state.Map.GetEnemyAt(checkX, checkY) != null && (checkX != startX || checkY != startY)))
            {
                break;
            }

            var player = state.GetAllActivePlayers().FirstOrDefault(p => p.X == checkX && p.Y == checkY);
            if (player != null)
            {
                distance = step;
                return player;
            }
        }
        return null;
    }

    public bool IsSurroundedByPlayers(Enemy enemy, GameState state, int requiredCount = 4)
    {
        int surroundingPlayers = 0;
        for (int i = 0; i < 4; i++)
        {
            int nx = enemy.X + Dx[i];
            int ny = enemy.Y + Dy[i];
            if (state.GetAllActivePlayers().Any(p => p.X == nx && p.Y == ny))
                surroundingPlayers++;
        }
        return surroundingPlayers >= requiredCount;
    }
}