namespace AlchemyRPG;

/// <summary>
/// Defines the contract for all executable actions in the game.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Evaluates whether the command is allowed to execute in the current game state for a specific player.
    /// </summary>
    bool CanExecute(GameState state, Player executor);

    /// <summary>
    /// Executes the action for the specified player.
    /// </summary>
    void Execute(GameState state, Player executor);
}