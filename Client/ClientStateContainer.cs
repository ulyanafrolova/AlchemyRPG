namespace AlchemyRPG;

/// <summary>
/// Represents a thread-safe container for the client's game state and error information.
/// It acts as a shared memory bridge between the asynchronous network receiver task and the main rendering loop.
/// </summary>
public class ClientStateContainer
{
    private GameStateDTO? _state;
    private string? _errorTitle;
    private string? _errorMessage;
    private readonly object _lock = new();

    /// <summary>
    /// Retrieves the most recent snapshot of the game state in a thread-safe manner.
    /// </summary>
    /// <returns>The current game state data transfer object, or null if no state has been received yet.</returns>
    public GameStateDTO? GetState()
    {
        lock (_lock) return _state;
    }

    /// <summary>
    /// Safely updates the currently stored game state snapshot with a new one received from the server.
    /// </summary>
    /// <param name="state">The new game state snapshot to store.</param>
    public void UpdateState(GameStateDTO state)
    {
        lock (_lock) _state = state;
    }

    /// <summary>
    /// Gets a value indicating whether a fatal error has occurred and is currently stored in the container.
    /// </summary>
    public bool HasError
    {
        get { lock (_lock) return _errorTitle != null; }
    }

    /// <summary>
    /// Retrieves the stored fatal error details safely.
    /// </summary>
    /// <returns>A tuple containing the error title and error message. Returns default "Unknown" values if no error is set.</returns>
    public (string Title, string Message) GetError()
    {
        lock (_lock) return (_errorTitle ?? "Unknown", _errorMessage ?? "Unknown error");
    }

    /// <summary>
    /// Safely sets a fatal error state, which will eventually trigger the client loop to display an error screen and halt.
    /// </summary>
    /// <param name="title">The title of the error.</param>
    /// <param name="message">The detailed description of the error.</param>
    public void SetFatalError(string title, string message)
    {
        lock (_lock)
        {
            _errorTitle = title;
            _errorMessage = message;
        }
    }
}