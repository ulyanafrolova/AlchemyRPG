namespace AlchemyRPG;

/// <summary>
/// Provides a static interface for initializing and accessing the application's logger instance.
/// </summary>
public static class GameLogger
{
    private static ILogger? _instance;

    public static void Initialize(ILogger logger)
    {
        _instance = logger;
    }

    public static ILogger Instance => _instance ?? throw new System.Exception("Logger not initialized!");
}