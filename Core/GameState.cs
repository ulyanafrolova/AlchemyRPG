namespace AlchemyRPG;

/// <summary>
/// Represents the current global state of the game.
/// This class acts as a data container passed around to different objects 
/// so they can interact with the player, the map, and the event log.
/// </summary>
public class GameState
{
    /// <summary>
    /// Gets or sets the main player character. 
    /// The 'required' modifier ensures the player is initialized when the state is created.
    /// </summary>
    public required Player Player { get; set; }

    /// <summary>
    /// Gets or sets the game map, including the grid layout and all items on the ground.
    /// The 'required' modifier ensures the map is initialized when the state is created.
    /// </summary>
    public required Map Map { get; set; }

    /// <summary>
    /// Gets or sets the system log message displayed at the bottom of the screen.
    /// Used to inform the player about game events, errors, or interaction feedback.
    /// </summary>
    public string Log { get; set; } = "Welcome to the game!";

    /// <summary>
    /// Gets or sets the dynamically generated tutorial instructions displayed to the player.
    /// These instructions adapt based on the contents of the current dungeon 
    /// (e.g., hiding the 'Pick Up' prompt if no items were spawned during map generation).
    /// </summary>
    public string Instructions { get; set; } = "";

    /// <summary>
    /// Stores the dynamically generated "How to play" text (rules and lore).
    /// </summary>
    public string TutorialText { get; set; } = "";

    public bool IsGameOver { get; set; } = false;
}