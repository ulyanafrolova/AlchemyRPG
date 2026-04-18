namespace AlchemyRPG;

/// <summary>
/// Represents a command to move the player by a specific offset on the map.
/// Handles boundary and wall collision detection before applying the movement.
/// </summary>
public class MoveCommand : ICommand
{
    private readonly int _dx;
    private readonly int _dy;

    /// <summary>
    /// Initializes a new instance of the MoveCommand.
    /// </summary>
    /// <param name="dx">The horizontal movement offset (-1 for left, 1 for right, 0 for none).</param>
    /// <param name="dy">The vertical movement offset (-1 for up, 1 for down, 0 for none).</param>
    public MoveCommand(int dx, int dy)
    {
        _dx = dx;
        _dy = dy;
    }

    /// <summary>
    /// Verifies if the target destination is walkable before modifying the player's coordinates.
    /// </summary>
    public bool CanExecute(GameState state)
    {
        int targetX = state.Player.X + _dx;
        int targetY = state.Player.Y + _dy;
        return targetX >= 0 && targetX < state.Map.Width && targetY >= 0 && targetY < state.Map.Height;
    }

    /// <summary>
    /// Executes the movement action. 
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    public void Execute(GameState state)
    {
        int targetX = state.Player.X + _dx;
        int targetY = state.Player.Y + _dy;

        var enemy = state.Map.GetEnemyAt(targetX, targetY);

        if (enemy != null)
        {
            state.Player.LogMessage = $"{enemy.Name} blocks your path! Presss [{Keybinds.Attack}] to fight.";
        }
        else if (state.Map.Grid[targetY, targetX] != Tiles.Wall)
        {
            state.Player.Move(_dx, _dy);
        }
        else
        {
            GameLogger.Instance.Log(LogType.Movement, $"{state.Player.Name} bumped into a cold stone wall.");
        }
    }
}