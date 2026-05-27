namespace AlchemyRPG;

/// <summary>
/// An infrastructure Observer component. Listens to raw domain events 
/// and formats them into human-readable text for the in-game UI event buffer and the system log.
/// </summary>
public class GameEventLogger : IObserver<EnemyHeardNoiseData>, IObserver<PlayerHeardNoiseData>, IObserver<SystemLogData>
{
    private readonly EventLog _eventLog;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventLogger"/> class.
    /// </summary>
    /// <param name="eventLog">The rolling buffer displayed in the game UI.</param>
    /// <param name="logger">The underlying system/file logger.</param>
    public GameEventLogger(EventLog eventLog, ILogger logger)
    {
        _eventLog = eventLog;
        _logger = logger;
    }

    /// <summary>
    /// Handles the event when an enemy detects a noise.
    /// </summary>
    /// <param name="data">The data packet containing the noise and listener coordinates.</param>
    public void OnNext(EnemyHeardNoiseData data)
    {
        string message = $"[{data.Species} at {data.EnemyX},{data.EnemyY}] " +
                         $"Heard a noise from {data.SourceX},{data.SourceY} " +
                         $"(Distance: {data.Distance} steps).";
        _eventLog.Push(message);
        _logger.Log(LogType.System, message);
    }

    /// <summary>
    /// Handles the event when a player detects a noise.
    /// </summary>
    /// <param name="data">The data packet containing the noise source and distance.</param>
    public void OnNext(PlayerHeardNoiseData data)
    {
        string message = $"{data.ListenerName} heard a noise from {data.SourceX},{data.SourceY}.";
        _eventLog.Push(message);
        _logger.Log(LogType.System, message);
    }

    /// <summary>
    /// Passes raw system log data directly to the underlying logger implementation.
    /// </summary>
    /// <param name="data">The system log message and category.</param>
    public void OnNext(SystemLogData data)
    {
        _logger.Log(data.Type, data.Message);
    }
}