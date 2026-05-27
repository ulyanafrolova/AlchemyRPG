using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// A thread-safe logging implementation that writes diagnostic and event data to a physical file 
/// while maintaining a rolling in-memory buffer for quick access (e.g., for the in-game Journal).
/// </summary>
public class FileLogger : ILogger, IDisposable
{
    private readonly Queue<LogEntry> _memoryBuffer = new();
    private readonly int _maxBufferSize = 100;
    private readonly StreamWriter _fileWriter;
    
    // Dedicated lock object for the memory buffer to prevent deadlocks with the file writer.
    private readonly object _syncRoot = new();

    /// <summary>Gets the absolute path to the active log file on disk.</summary>
    public string SavedFilePath { get; }
    
    public string? GetLogFilePath() => SavedFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLogger"/> class, creating the log directory if needed.
    /// </summary>
    /// <param name="directory">The target directory for the log file.</param>
    /// <param name="playerName">A string identifier (e.g., "Server" or a player name) prepended to the file name.</param>
    public FileLogger(string directory, string playerName)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        SavedFilePath = Path.Combine(directory, $"{playerName}_{timestamp}.txt");

        // AutoFlush ensures logs are written to disk immediately, preventing data loss on crash.
        _fileWriter = new StreamWriter(SavedFilePath, append: true) { AutoFlush = true };
    }

    /// <summary>
    /// Records a new log entry to both the in-memory buffer and the persistent file.
    /// </summary>
    /// <param name="type">The category or severity of the log.</param>
    /// <param name="message">The text message to record.</param>
    public void Log(LogType type, string message)
    {
        var entry = new LogEntry(type, message);
        
        lock (_syncRoot)
        {
            if (_memoryBuffer.Count >= _maxBufferSize)
            {
                _memoryBuffer.Dequeue();
            }
            _memoryBuffer.Enqueue(entry);
        }

        try
        {
            // Lock the stream writer directly to ensure thread-safe file I/O operations.
            lock (_fileWriter)
            {
                _fileWriter.WriteLine(entry.ToString());
            }
        }
        catch { /* Suppress file IO exceptions to prevent crashing the game server */ }
    }

    /// <summary>
    /// Retrieves a snapshot of the entire active memory buffer.
    /// </summary>
    public IReadOnlyList<LogEntry> GetFullMemoryBuffer()
    {
        lock (_syncRoot)
        {
            return _memoryBuffer.ToList();
        }
    }

    /// <summary>
    /// Retrieves the specified number of the most recent log entries.
    /// </summary>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    public IReadOnlyList<LogEntry> GetRecentLogs(int count)
    {
        lock (_syncRoot)
        {
            return _memoryBuffer.Skip(Math.Max(0, _memoryBuffer.Count - count)).ToList();
        }
    }

    /// <summary>
    /// Releases the unmanaged resources utilized by the StreamWriter.
    /// </summary>
    public void Dispose()
    {
        lock (_fileWriter)
        {
            _fileWriter?.Close();
        }
    }
}