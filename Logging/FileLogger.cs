namespace AlchemyRPG;

/// <summary>
/// Provides a logger that writes log entries to a file and maintains a memory buffer of recent logs for in-memory
/// access.
/// </summary>
public class FileLogger : ILogger, IDisposable
{
    public string? GetLogFilePath() => SavedFilePath;
    
    /// <summary>
    /// In-memory FIFO buffer storing the most recent <see cref="_maxBufferSize"/> <see cref="LogEntry"/> instances.
    /// This buffer is intended for quick UI access without reading the log file from disk.
    /// </summary>
    private readonly Queue<LogEntry> _memoryBuffer = new();

    /// <summary>
    /// Maximum number of entries to keep in the in-memory buffer
    /// </summary>
    private readonly int _maxBufferSize = 100; 
    
    /// <summary>
    /// Stream writer used to append log entries to the persistent log file on disk. Opened in append mode and
    /// configured with <see cref="StreamWriter.AutoFlush"/> enabled so entries are flushed immediately.
    /// </summary>
    private readonly StreamWriter _fileWriter;

    /// <summary>
    /// Full path to the file where log entries are being written
    /// </summary>
    public string SavedFilePath { get; }

    /// <summary>
    /// Creates a new instance of <see cref="FileLogger"/>.
    /// </summary>
    /// <param name="directory">Directory where the log file will be created. If the directory does not exist it
    /// will be created.</param>
    /// <param name="playerName">Used as part of the log file name to identify which player's session the log
    /// belongs to.</param>
    public FileLogger(string directory, string playerName)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        SavedFilePath = Path.Combine(directory, $"{playerName}_{timestamp}.txt");

        _fileWriter = new StreamWriter(SavedFilePath, append: true)
        {
            AutoFlush = true 
        };
    }

    /// <summary>
    /// Creates a new log entry and records it both in the in-memory buffer and to the persistent log file
    /// </summary>
    /// <param name="type">Severity or category of the log entry.</param>
    /// <param name="message">The message text to record.</param>
    public void Log(LogType type, string message)
    {
        var entry = new LogEntry(type, message);

        if (_memoryBuffer.Count >= _maxBufferSize)
        {
            _memoryBuffer.Dequeue();
        }
        _memoryBuffer.Enqueue(entry);

        try
        {
            _fileWriter.WriteLine(entry.ToString());
        }
        catch { }
    }

    /// <summary>
    /// Returns a snapshot of the entire in-memory buffer as a read-only list
    /// </summary>
    /// <returns>A read-only list of all <see cref="LogEntry"/> items currently stored in memory.</returns>
    public IReadOnlyList<LogEntry> GetFullMemoryBuffer() => _memoryBuffer.ToList();

    /// <summary>
    /// Returns the most recent <paramref name="count"/> entries from the in-memory buffer. If <paramref name="count"/>
    /// is greater than the number of items in the buffer, the entire buffer is returned.
    /// </summary>
    /// <param name="count">Number of recent entries to retrieve.</param>
    /// <returns>A read-only list of up to <paramref name="count"/> most recent <see cref="LogEntry"/> items.</returns>
    public IReadOnlyList<LogEntry> GetRecentLogs(int count) =>
        _memoryBuffer.Skip(Math.Max(0, _memoryBuffer.Count - count)).ToList();

    /// <summary>
    /// Releases resources used by the logger
    /// </summary>
    public void Dispose()
    {
        _fileWriter?.Close();
    }
}