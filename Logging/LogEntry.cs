using System;

namespace AlchemyRPG;

/// <summary>
/// Represents a single log entry containing a timestamp, log type, and message
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets the date and time when the event occurred
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the type of the log entry
    /// </summary>
    public LogType Type { get; }

    /// <summary>
    /// Gets the message associated with this instance
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the LogEntry class with the specified log type and message
    /// </summary>
    /// <param name="type">The type of the log entry, indicating the severity or category of the log</param>
    /// <param name="message">The message to associate with the log entry. Cannot be null.</param>
    public LogEntry(LogType type, string message)
    {
        Timestamp = DateTime.Now;
        Type = type;
        Message = message;
    }

    /// <summary>
    /// Returns a formatted string that represents the current log entry, including the timestamp, log type, and
    /// message.
    /// </summary>
    /// <returns>A string in the format "[HH:mm:ss] [Type] Message" representing the log entry.</returns>
    public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{Type}] {Message}";
}