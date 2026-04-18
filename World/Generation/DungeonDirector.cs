namespace AlchemyRPG;

/// <summary>
/// Acts as the "Director" in the Builder Design Pattern.
/// This class defines predefined strategies for automatically building dungeons  
/// </summary>
public class DungeonDirector
{
    private readonly IDungeonBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the DungeonDirector.
    /// </summary>
    /// <param name="builder">The builder implementation that will execute the construction steps.</param>
    public DungeonDirector(IDungeonBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Constructs a dungeon using the specified theme factory, applying theme-specific configuration and populating the
    /// dungeon with themed loot and enemies.
    /// </summary>
    /// <param name="themeFactory">The factory that defines the theme and configuration for the dungeon. Cannot be null.</param>
    public void ConstructThemedDungeon(IThemeFactory themeFactory)
    {
        themeFactory.ConfigureBuilder(_builder);

        _builder.ApplyModifier(new ThemePopulatorModifier(themeFactory, lootCount: 10, enemyCount: 6));
    }
    public IDungeonBuilder GetBuilder() => _builder;
}