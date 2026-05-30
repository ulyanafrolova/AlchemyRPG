namespace AlchemyRPG;

/// <summary>
/// Represents a command to move the player by a specific offset on the map.
/// Handles boundary checks and collision detection with walls, enemies, and other players.
/// </summary>
public class MoveCommand : ICommand
{
    private readonly int _dx;
    private readonly int _dy;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveCommand"/> class.
    /// </summary>
    /// <param name="dx">The horizontal displacement (-1, 0, or 1).</param>
    /// <param name="dy">The vertical displacement (-1, 0, or 1).</param>
    public MoveCommand(int dx, int dy)
    {
        _dx = dx;
        _dy = dy;
    }

    /// <summary>
    /// Validates the move attempt. Ensures the player is alive, the move is within valid map boundaries, 
    /// and the input is not an illegal diagonal or long-distance jump.
    /// </summary>
    public bool CanExecute(GameState state, Player executor)
    {
        if (executor.IsDead) return false;

        // Security check: Prevent diagonal movement (if prohibited) or teleportation (dx/dy > 1)
        if (Math.Abs(_dx) > 1 || Math.Abs(_dy) > 1 || (Math.Abs(_dx) == 1 && Math.Abs(_dy) == 1))
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.System,
                $"[SECURITY ALERT] Player {executor.Name} attempted illegal movement delta ({_dx}, {_dy})."));
            return false;
        }

        int targetX = executor.X + _dx;
        int targetY = executor.Y + _dy;

        // Ensure target is within map bounds
        if (targetX < 0 || targetX >= state.Map.Width || targetY < 0 || targetY >= state.Map.Height)
            return false;

        return true;
    }

    /// <summary>
    /// Executes the movement. Handles collision feedback if the path is blocked by enemies, 
    /// other players, or walls. If clear, updates the executor's position.
    /// </summary>
    public void Execute(GameState state, Player executor)
    {
        int targetX = executor.X + _dx;
        int targetY = executor.Y + _dy;

        var enemy = state.Map.GetEnemyAt(targetX, targetY);
        var otherPlayer = state.GetAllActivePlayers()
            .FirstOrDefault(p => p.X == targetX && p.Y == targetY && p != executor);

        if (enemy != null)
        {
            executor.SetLogMessage($"{enemy.Name} blocks your path! Press [{Keybinds.Attack}] to fight.");
        }
        else if (otherPlayer != null)
        {
            executor.SetLogMessage($"{otherPlayer.Name} stands in your way.");
        }
        else if (!state.Map.GetTileAt(targetX, targetY).IsWalkable)
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.Movement,
                $"{executor.Name} bumped into a cold stone wall."));
        }
        else
        {
            executor.Move(_dx, _dy);
        }
    }
}