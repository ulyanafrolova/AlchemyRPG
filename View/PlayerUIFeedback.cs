namespace AlchemyRPG;

/// <summary>
/// An infrastructure observer that monitors acoustic events specifically for player feedback.
/// It bridges the gap between the domain-level noise system and the player's UI log, 
/// ensuring that when a player "hears" a sound, they receive immediate visual feedback.
/// </summary>
public class PlayerUIFeedbackObserver : IObserver<PlayerHeardNoiseData>
{
    /// <summary>
    /// A reference to the active player collection, allowing the observer to look up 
    /// specific players by their identity to route feedback messages correctly.
    /// </summary>
    private readonly IReadOnlyDictionary<int, Player> _players;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerUIFeedbackObserver"/> class.
    /// </summary>
    /// <param name="players">The thread-safe dictionary of active players.</param>

    public PlayerUIFeedbackObserver(IReadOnlyDictionary<int, Player> players)
    {
        _players = players;
    }

    /// <summary>
    /// Reacts to a <see cref="PlayerHeardNoiseData"/> event.
    /// Locates the specific player who heard the noise and updates their personal log message 
    /// to provide direct feedback to their client UI.
    /// </summary>
    /// <param name="data">The event data containing the listener's identity and sound distance.</param>
    public void OnNext(PlayerHeardNoiseData data)
    {
        // Identify the player by name and update their local message buffer
        var player = _players.Values.FirstOrDefault(p => p.Name == data.ListenerName);

        if (player != null)
        {
            player.SetLogMessage($"*{player.Name} hear a noise {data.Distance} steps away!*");
        }
    }
}