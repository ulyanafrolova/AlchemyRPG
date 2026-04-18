namespace AlchemyRPG;

/// <summary>
/// A command that pauses the active game loop to display the dynamic tutorial screen.
/// </summary>
public class HelpCommand : ICommand
{
    /// <summary>
    /// Help can always be executed, regardless of the player's state or position.
    /// </summary>
    public bool CanExecute(GameState state) => true;

    /// <summary>
    /// Clears the screen, displays the dynamically generated tutorial text, and waits 
    /// for the player to acknowledge it before returning to the game cycle.
    /// </summary>
    public void Execute(GameState state)
    {
        Console.Clear();
        Console.WriteLine(state.TutorialText);
        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("Press any key to start/return to the game...");

        Console.ReadKey(true);
        Console.Clear();
    }
}