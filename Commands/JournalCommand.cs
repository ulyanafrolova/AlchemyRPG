namespace AlchemyRPG;

/// <summary>
/// Represents a command that displays the player's adventure journal, showing the full game log history.
/// </summary>
public class JournalCommand : ICommand
{
    public bool CanExecute(GameState state) => true;

    public void Execute(GameState state)
    {
        Console.Clear();
        Console.WriteLine("--- ADVENTURE JOURNAL ---\n");

        var fullHistory = GameLogger.Instance.GetFullMemoryBuffer();

        foreach (var log in fullHistory)
        {
            Console.WriteLine(log);
        }
        Console.WriteLine("\n-------------------------");

        Console.WriteLine("Press any key to return to the game...");

        Console.ReadKey(true);

        Console.Clear();
    }
}