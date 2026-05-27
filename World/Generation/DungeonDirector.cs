namespace AlchemyRPG;

/// <summary>
/// The "Director" in the Builder Design Pattern.
/// It encapsulates the high-level construction logic for various dungeon layouts.
/// Instead of building manually, the client interacts with the director to produce complex map configurations.
/// </summary>
public class DungeonDirector
{
    private readonly IDungeonBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonDirector"/> class.
    /// </summary>
    /// <param name="builder">The builder instance to orchestrate during the construction process.</param>
    public DungeonDirector(IDungeonBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Executes a standardized pipeline to build a fully realized dungeon.
    /// This includes applying structural layout modifiers, populating the map with loot, 
    /// and spawning themed enemies using the provided <see cref="IThemeFactory"/>.
    /// </summary>
    /// <param name="themeFactory">The abstract factory defining the visual and functional theme of the dungeon.</param>
    /// <param name="noiseEvents">Event bus for propagating sound events.</param>
    /// <param name="deathEvents">Event bus for propagating enemy death signals.</param>
    /// <param name="heardNoiseEvents">Event bus for enemy reactions to perceived sounds.</param>
    /// <param name="systemLogs">Event bus for system diagnostics.</param>
    public void ConstructThemedDungeon(
        IThemeFactory themeFactory,
        ISubject<NoiseData> noiseEvents,
        ISubject<EnemyDeathData> deathEvents,
        ISubject<EnemyHeardNoiseData> heardNoiseEvents,
        ISubject<SystemLogData> systemLogs)
    {
        // 1. Configure architectural modifiers via the factory (Rooms, Corridors, etc.)
        themeFactory.ConfigureBuilder(_builder);
        
        // 2. Populate the dungeon with content (Artifacts, Loot, Enemies)
        _builder.ApplyModifier(new ThemePopulatorModifier(
            themeFactory, 
            lootCount: 10, 
            enemyCount: 6,
            noiseEvents, 
            deathEvents, 
            heardNoiseEvents, 
            systemLogs));
    }

    /// <summary>
    /// Provides access to the underlying builder instance.
    /// </summary>
    public IDungeonBuilder GetBuilder() => _builder;
}