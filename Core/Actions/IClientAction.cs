namespace AlchemyRPG;

/// <summary>
/// Defines a common interface for client-side actions that do not require server communication.
/// These actions typically affect the local UI, input state, or client process lifecycle.
/// </summary>
public interface IClientAction
{
    /// <summary>
    /// Executes the specific client-side logic.
    /// </summary>
    /// <param name="client">The network client instance.</param>
    /// <param name="view">The current view implementation.</param>
    /// <param name="snapshot">The latest game state snapshot.</param>
    /// <param name="inputController">The input state machine that may need to be reset.</param>
    void Execute(NetworkClient client, IView view, GameStateDTO? snapshot, ref ClientInputController inputController);
}

/// <summary>
/// Triggers the shutdown of the network client, effectively disconnecting from the server.
/// </summary>
public class QuitAction : IClientAction
{
    public void Execute(NetworkClient client, IView view, GameStateDTO? snapshot, ref ClientInputController inputController)
    {
        client.Stop();
    }
}

/// <summary>
/// Resets the input state machine to the default <see cref="NormalState"/>.
/// Used when the player cancels a multi-step input sequence.
/// </summary>
public class ResetInputStateAction : IClientAction
{
    public void Execute(NetworkClient client, IView view, GameStateDTO? snapshot, ref ClientInputController inputController)
    {
        inputController = new ClientInputController();
    }
}

/// <summary>
/// Displays the full adventure journal to the user via the view.
/// </summary>
public class OpenJournalAction : IClientAction
{
    public void Execute(NetworkClient client, IView view, GameStateDTO? snapshot, ref ClientInputController inputController)
    {
        if (snapshot != null)
            view.RenderJournal(snapshot.FullJournal);
    }
}   

/// <summary>
/// Displays the dynamic tutorial or help screen to the user.
/// </summary>
public class ShowHelpAction : IClientAction
{
    public void Execute(NetworkClient client, IView view, GameStateDTO? snapshot, ref ClientInputController inputController)
    {
        view.RenderFullScreen("DUNGEON INSTRUCTIONS", view.TutorialText);
    }
}