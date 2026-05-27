namespace AlchemyRPG;

/// <summary>
/// A thread-safe circular buffer designed to store recent game events.
/// This class serves as a domain-level component (Model) representing the world's event history,
/// independent of any UI-specific presentation logic.
/// </summary>
public class EventLog
{
    /// <summary>
    /// Synchronization object to ensure thread-safe access to the internal queue.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// The internal collection storing log messages.
    /// </summary>
    private readonly Queue<string> _entries = new();

    /// <summary>
    /// The maximum number of log entries to retain.
    /// </summary>
    private readonly int _capacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLog"/> class with a specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of entries to keep in the buffer.</param>
    public EventLog(int capacity = 5)
    {
        _capacity = capacity;
    }

    /// <summary>
    /// Adds a new message to the log. If the capacity is exceeded, the oldest entry is removed.
    /// </summary>
    /// <param name="message">The text message to record.</param>
    public void Push(string message)
    {
        lock (_lock)
        {
            if (_entries.Count >= _capacity)
                _entries.Dequeue();
            _entries.Enqueue(message);
        }
    }

    /// <summary>
    /// Retrieves a read-only list of the most recent log entries.
    /// </summary>
    /// <returns>A list containing the current log entries.</returns>
    public IReadOnlyList<string> GetRecent()
    {
        lock (_lock) 
        { 
            return _entries.ToList(); 
        }
    }
}