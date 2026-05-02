namespace AlchemyRPG;

/// <summary>
/// Defines a contract for logging messages with various log types and retrieving log entries from memory buffers
/// </summary>
public interface ILogger
{
    void Log(LogType type, string message);
    IReadOnlyList<LogEntry> GetFullMemoryBuffer();
    IReadOnlyList<LogEntry> GetRecentLogs(int count);
    string? GetLogFilePath();
}