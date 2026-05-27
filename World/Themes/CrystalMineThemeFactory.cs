namespace AlchemyRPG;

/// <summary>
/// A concrete implementation of the <see cref="IThemeFactory"/> that generates a "Crystal Mine" biome.
/// This theme is characterized by a massive central cavern, heavy stone and crystal-based enemies, 
/// and mining-appropriate loot like heavy axes.
/// </summary>
public class CrystalMineThemeFactory : IThemeFactory
{
    /// <summary>
    /// Retrieves the atmospheric narrative text to display when the player enters this specific theme.
    /// </summary>
    /// <returns>A descriptive string setting the mood for the crystal mines.</returns>
    public string GetWelcomeMessage() => "\nThe blinding glare of magical crystals lights up the darkness...";

    /// <summary>
    /// Dictates the architectural layout of the crystal mine dungeon.
    /// Uses a massive central cavern with a few smaller peripheral rooms attached.
    /// </summary>
    /// <param name="builder">The dungeon builder instance to apply the layout modifiers to.</param>
    public void ConfigureBuilder(IDungeonBuilder builder)
    {
        builder.CreateFilled(40, 20).ApplyModifier(new CentralRoomModifier(12, 8)).ApplyModifier(new RoomsModifier(3));
    }

    /// <summary>
    /// Creates a theme-appropriate enemy for the crystal mine biome.
    /// </summary>
    /// <param name="rand">The random number generator used to select between different enemy types.</param>
    /// <returns>An <see cref="Enemy"/> instance (e.g., a Crystal Basilisk or a Stone Gargoyle).</returns>
    public Enemy CreateEnemy(int index, ISubject<NoiseData> noiseEvents, ISubject<EnemyDeathData> deathEvents, ISubject<EnemyHeardNoiseData> heardNoiseEvents, ISubject<SystemLogData> systemLogs)
    {
        return index % 2 == 0
            ? new Enemy("Crystal Basilisk", "Basilisk", 45, 25, 6, noiseEvents, deathEvents, heardNoiseEvents, systemLogs, new CowardlyBehavior())
            : new Enemy("Stone Gargoyle", "Gargoyle", 80, 15, 10, noiseEvents, deathEvents, heardNoiseEvents, systemLogs, new AggressiveBehavior());
    }


    /// <summary>
    /// Generates standard thematic loot for the player to find scattered in the mines.
    /// </summary>
    /// <param name="rand">The random number generator used to determine the exact item dropped.</param>
    /// <returns>An <see cref="IItem"/> representing basic loot (e.g., Two-Handed Axe or a Skull).</returns>
    public IItem CreateLoot(Random rand)
    {
        return rand.Next(2) == 0 ? new TwoHandedAxe() : new Skull();
    }

    /// <summary>
    /// Constructs the unique, high-tier artifact specifically tied to the crystal mine theme.
    /// </summary>
    /// <returns>An <see cref="IWeapon"/> representing the artifact. In this case, a massive Resonating Hammer (represented by an upgraded Two-Handed Axe).</returns>
    public IWeapon CreateArtifact()
    {
        return new StrongModifier(new TwoHandedAxe());
    }
}