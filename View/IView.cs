namespace AlchemyRPG;

/// <summary>
/// Defines the contract for the UI rendering layer of the client application.
/// This interface abstracts the underlying rendering technology (e.g., Console, WPF, or Unity),
/// allowing the game to remain decoupled from the specific implementation.
/// </summary>
public interface IView
{
    /// <summary>
    /// Gets a value indicating whether the view has been successfully initialized 
    /// with the initial server configuration and data.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the tutorial text provided by the server, used for help screens and instructions.
    /// </summary>
    string TutorialText { get; }

    /// <summary>
    /// Performs an initial setup using the primary game state data.
    /// </summary>
    /// <param name="stateDto">The initial state data transfer object.</param>
    void InitializeFromState(GameStateDTO stateDto);

    /// <summary>
    /// Renders the current frame of the game world based on the provided state snapshot.
    /// </summary>
    /// <param name="stateDto">The current game state to render.</param>
    /// <param name="localPrompt">An optional UI prompt (e.g., input guidance) to display to the user.</param>
    void Render(GameStateDTO stateDto, string localPrompt = "");

    /// <summary>
    /// Displays a full-screen modal (e.g., for help screens or error messages) 
    /// that pauses standard game rendering.
    /// </summary>
    /// <param name="title">The title of the full-screen view.</param>
    /// <param name="content">The body content to be displayed.</param>
    void RenderFullScreen(string title, string content);

    /// <summary>
    /// Displays a temporary message or notification to the user outside of the main render loop.
    /// </summary>
    /// <param name="message">The notification text.</param>
    void DisplayMessage(string message);

    /// <summary>
    /// Renders the adventure journal interface, showing the history of logged game events.
    /// </summary>
    /// <param name="entries">The list of string entries to display in the journal.</param>
    void RenderJournal(IReadOnlyList<string> entries);
}