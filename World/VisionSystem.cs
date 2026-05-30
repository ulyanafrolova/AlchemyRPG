namespace AlchemyRPG;

/// <summary>
/// Defines the contract for a vision system that determines which players are visible to an enemy based on line of sight and obstacles.
/// </summary>
public interface IVisionService
{
    List<Player> GetVisiblePlayers(Enemy enemy, GameState state, int sightRange = 8);
    Player? GetVisiblePlayer(Enemy enemy, GameState state, int sightRange = 8);
}

/// <summary>
/// Implements the IVisionService to provide line-of-sight calculations for enemies to detect players in the game world.
/// </summary>
public class VisionSystem : IVisionService
{
    private static readonly int[] Dx = { 0, 0, -1, 1 };
    private static readonly int[] Dy = { -1, 1, 0, 0 };
    /// <summary>
    /// Determines which players are visible to the given enemy based on line of sight and obstacles in the game world.
    /// </summary>
    public List<Player> GetVisiblePlayers(Enemy enemy, GameState state, int sightRange = 8)
    {
        var visiblePlayers = new List<Player>();
        for (int dir = 0; dir < 4; dir++)
        {
            for (int step = 1; step <= sightRange; step++)
            {
                int checkX = enemy.X + (Dx[dir] * step);
                int checkY = enemy.Y + (Dy[dir] * step);

                if (checkX < 0 || checkX >= state.Map.Width || checkY < 0 || checkY >= state.Map.Height)
                    break;

                if (!state.Map.GetTileAt(checkX, checkY).IsWalkable ||
                   (state.Map.GetEnemyAt(checkX, checkY) != null && (checkX != enemy.X || checkY != enemy.Y)))
                {
                    break;
                }

                var player = state.GetAllActivePlayers().FirstOrDefault(p => p.X == checkX && p.Y == checkY);
                if (player != null)
                {
                    if (!visiblePlayers.Contains(player))
                        visiblePlayers.Add(player);
                    break;
                }
            }
        }
        return visiblePlayers;
    }

    /// <summary>
    /// Determines the closest visible player to the given enemy based on line of sight and obstacles in the game world.
    /// </summary>
    public Player? GetVisiblePlayer(Enemy enemy, GameState state, int sightRange = 8)
    {
        Player? closest = null;
        int minDistance = int.MaxValue;

        for (int dir = 0; dir < 4; dir++)
        {
            for (int step = 1; step <= sightRange; step++)
            {
                int checkX = enemy.X + (Dx[dir] * step);
                int checkY = enemy.Y + (Dy[dir] * step);

                if (checkX < 0 || checkX >= state.Map.Width || checkY < 0 || checkY >= state.Map.Height)
                    break;

                if (!state.Map.GetTileAt(checkX, checkY).IsWalkable ||
                   (state.Map.GetEnemyAt(checkX, checkY) != null && (checkX != enemy.X || checkY != enemy.Y)))
                {
                    break;
                }

                var player = state.GetAllActivePlayers().FirstOrDefault(p => p.X == checkX && p.Y == checkY);
                if (player != null)
                {
                    if (step < minDistance)
                    {
                        minDistance = step;
                        closest = player;
                    }
                    break;
                }
            }
        }
        return closest;
    }
}