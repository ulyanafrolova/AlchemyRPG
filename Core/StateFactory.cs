using System;
using System.Collections.Generic;

namespace AlchemyRPG;

/// <summary>
/// A factory class responsible for assembling the initial authoritative state of the game.
/// It configures the map builder, instantiates the requested dungeon theme, and wires up the event observation pipelines.
/// </summary>
public static class StateFactory
{
    /// <summary>
    /// Constructs a fully prepared <see cref="GameState"/> instance ready to be used by the engine.
    /// </summary>
    /// <param name="config">The loaded game configuration settings.</param>
    /// <param name="logger">The system logger used for initialization diagnostics.</param>
    /// <returns>A fully initialized game state containing the map and event buses.</returns>
    public static GameState CreateInitialState(GameConfig config, ILogger logger)
    {
        var builder = new DungeonBuilder();
        var director = new DungeonDirector(builder);

        // Registry mapping theme names to their corresponding factory constructors
        var themeRegistry = new Dictionary<string, Func<IThemeFactory>>(StringComparer.OrdinalIgnoreCase)
        {
            { "greenhouse", () => new GreenhouseThemeFactory() },
            { "laboratory", () => new LaboratoryThemeFactory() },
            { "crystalmine", () => new CrystalMineThemeFactory() }
        };

        if (!themeRegistry.TryGetValue(config.DungeonTheme, out var createTheme))
        {
            logger.Log(LogType.System, $"Unknown theme '{config.DungeonTheme}'. Defaulting to Laboratory.");
            createTheme = () => new LaboratoryThemeFactory();
        }

        IThemeFactory activeTheme = createTheme();
        
        // Initialize event buses (Subject/Observer pattern)
        ISubject<NoiseData> noiseEvents = new Subject<NoiseData>();
        ISubject<EnemyDeathData> deathEvents = new Subject<EnemyDeathData>();
        ISubject<EnemyHeardNoiseData> heardNoiseEvents = new Subject<EnemyHeardNoiseData>();
        ISubject<PlayerHeardNoiseData> playerHeardNoiseEvents = new Subject<PlayerHeardNoiseData>();
        var tempSystemLogs = new Subject<SystemLogData>();

        director.ConstructThemedDungeon(
            activeTheme,
            noiseEvents,
            deathEvents,
            heardNoiseEvents,
            tempSystemLogs
        );
        
        Map generatedMap = builder.GetMap();

        var state = new GameState
        {
            Config = config,
            Map = generatedMap,
            NoiseEvents = noiseEvents,
            DeathEvents = deathEvents,
            HeardNoiseEvents = heardNoiseEvents,
            PlayerHeardNoiseEvents = playerHeardNoiseEvents,
            Acoustic = new AcousticSystem(),
            SystemLogs = tempSystemLogs,
            TutorialText = builder.GetTutorialText(),
            ControlsText = builder.GetInstructions(),
        };

        // Attempt to find a safe spawning coordinate; default to (1, 1) if unavailable
        int startX = 1, startY = 1;
        if (!generatedMap.IsWalkable(startX, startY))
        {
            var safeSpawn = generatedMap.GetRandomWalkableTile(Random.Shared);
            if (safeSpawn.HasValue)
            {
                startX = safeSpawn.Value.x;
                startY = safeSpawn.Value.y;
            }
        }

        var eventLogger = new GameEventLogger(state.EventLog, logger);
        heardNoiseEvents.Subscribe(eventLogger);
        playerHeardNoiseEvents.Subscribe(eventLogger);
        tempSystemLogs.Subscribe(eventLogger);
        
        var uiObserver = new PlayerUIFeedbackObserver(state.Players);
        playerHeardNoiseEvents.Subscribe(uiObserver);

        return state;
    }
}