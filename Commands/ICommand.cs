namespace AlchemyRPG;

/// <summary>
/// Defines the contract for all keyboard actions in the game.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Evaluates whether the command is allowed to execute in the current game state.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <returns>True if execution is allowed; otherwise, false.</returns>
    bool CanExecute(GameState state);

    /// <summary>
    /// Executes the action. Should only be called if CanExecute returns true.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    void Execute(GameState state);
}