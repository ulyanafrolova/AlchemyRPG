using System;

namespace AlchemyRPG;

/// <summary>
/// Represents the main game engine. 
/// Handles the initialization of the game world, manages the main game loop.
/// </summary>
public class Game
{
    /// <summary>
    /// Global state of the game: the map, player, and logs.
    /// </summary>
    private readonly GameState _state;

    /// <summary>
    /// Flag indicating whether the main game loop should continue running.
    /// </summary>
    private bool _isRunning = true;

    /// <summary>
    /// The input handler responsible for mapping keyboard keys to specific commands.
    /// </summary>
    private readonly InputHandler _inputHandler;

    /// <summary>
    /// Initializes a new instance of the Game class.
    /// Sets up the console, constructs the dungeon using the Builder pattern, 
    /// and prepares the initial game state.
    /// </summary>
    public Game()
    {
        // Support special characters ('█' or '¶')
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;

        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(Config.WindowWidth, Config.WindowHeight);
                Console.SetBufferSize(Config.WindowWidth, Config.WindowHeight);
            }
            catch { }
        }

        GameConfig config = GameConfig.Load("config.json");

        GameLogger.Initialize(new FileLogger(config.LogDirectory, config.PlayerName));
        GameLogger.Instance.Log(LogType.System, "Game Engine initialized.");

        // Initialize the command pattern invoker
        _inputHandler = new InputHandler();

        ISubject<NoiseData> noiseEvents = new Subject<NoiseData>();
        ISubject<EnemyDeathData> deathEvents = new Subject<EnemyDeathData>();

        // Use the Builder pattern to generate the dungeon
        var builder = new DungeonBuilder();
        var director = new DungeonDirector(builder);

        var themeRegistry = new Dictionary<string, Func<IThemeFactory>>(StringComparer.OrdinalIgnoreCase)
        {
            { "greenhouse", () => new GreenhouseThemeFactory() },
            { "laboratory", () => new LaboratoryThemeFactory() },
            { "crystalmine",      () => new CrystalMineThemeFactory() }
        };

        if (!themeRegistry.TryGetValue(config.DungeonTheme, out var createTheme))
        {
            GameLogger.Instance.Log(LogType.System, $"Unknown theme '{config.DungeonTheme}'. Defaulting to Laboratory.");
            createTheme = () => new LaboratoryThemeFactory();
        }

        IThemeFactory activeTheme = createTheme();

        director.ConstructThemedDungeon(activeTheme, noiseEvents, deathEvents);
        Map generatedMap = builder.GetMap();
        int startX = 1;
        int startY = 1;
        if (!generatedMap.IsWalkable(startX, startY))
        {
            var safeSpawn = generatedMap.GetRandomWalkableTile(new Random());
            startX = safeSpawn.x;
            startY = safeSpawn.y;
        }

        _state = new GameState
        {
            Config = config,
            Player = new Player(config.PlayerName, startX, startY),
            Map = generatedMap,
            Instructions = builder.GetInstructions(),
            TutorialText = builder.GetTutorialText(),
            NoiseEvents = noiseEvents,
            DeathEvents = deathEvents
        };
    }
    /// <summary>
    /// Starts the main game loop using a non-blocking Real-Time architecture.
    /// </summary>
    public void Run()
    {
        Console.Clear();
        new HelpCommand().Execute(_state);

        DateTime lastEnemyMove = DateTime.Now;
        TimeSpan enemyMoveInterval = TimeSpan.FromMilliseconds(800);

        Console.SetCursorPosition(0, 0);
        _state.Map.Draw(_state);

        while (_isRunning && !_state.IsGameOver)
        {
            bool needsRedraw = false;

            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);

                if (_state.IsWaitingForSecondaryInput && _state.PendingAction != null)
                {
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        _state.IsWaitingForSecondaryInput = false;
                        _state.PendingAction = null;
                        GameLogger.Instance.Log(LogType.System, "Action cancelled.");
                    }
                    else
                    {
                        _state.PendingAction(keyInfo.Key);
                    }
                }
                else
                {
                    _isRunning = _inputHandler.HandleInput(keyInfo.Key, _state);
                }
                needsRedraw = true;
            }

            if (DateTime.Now - lastEnemyMove >= enemyMoveInterval)
            {
                for (int i = _state.Map.Enemies.Count - 1; i >= 0; i--)
                {
                    var enemy = _state.Map.Enemies[i];
                    if (!enemy.IsDead)
                    {
                        enemy.Update(_state, Random.Shared);
                    }
                }
                lastEnemyMove = DateTime.Now;
                needsRedraw = true;
            }

            if (needsRedraw)
            {
                Console.SetCursorPosition(0, 0);
                _state.Map.Draw(_state);
            }

            Thread.Sleep(30);
        }

        if (_state.IsGameOver)
        {
            Console.Clear();
            Console.WriteLine("\n\n-----------------------------------------");
            Console.WriteLine("               GAME OVER                 ");
            Console.WriteLine("       You have lost in the battle!      ");
            Console.WriteLine("-----------------------------------------\n\n");
            string? logPath = GameLogger.Instance.GetLogFilePath();
            if (!string.IsNullOrEmpty(logPath))
            {
                Console.WriteLine($"\nFull adventure log saved to: {logPath}");
            }
        }
    }
}