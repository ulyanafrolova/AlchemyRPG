namespace AlchemyRPG;

/// <summary>
/// A concrete implementation of the <see cref="IThemeFactory"/> that generates an "Abandoned Laboratory" biome.
/// This theme is characterized by isolated research chambers, bubbling alchemical hazards, 
/// and artificial constructs such as flesh golems and acid slimes.
/// </summary>
public class LaboratoryThemeFactory : IThemeFactory
{
    /// <summary>
    /// Retrieves the atmospheric narrative text to display when the player enters this specific theme.
    /// </summary>
    /// <returns>A descriptive string setting the mood for the alchemical laboratory.</returns>
    public string GetWelcomeMessage() => "\nThe sound of shattering glass and bubbling acids echoes around...";

    /// <summary>
    /// Dictates the architectural layout of the laboratory dungeon.
    /// Configures the map with multiple isolated rooms connected by narrow corridors, 
    /// simulating a secure, structured research facility.
    /// </summary>
    /// <param name="builder">The dungeon builder instance used to apply structural modifiers.</param>
    public void ConfigureBuilder(IDungeonBuilder builder)
    {
        builder.CreateFilled(40, 20).ApplyModifier(new RoomsModifier(6)).ApplyModifier(new CorridorsModifier());
    }

    /// <summary>
    /// Creates a theme-appropriate enemy for the laboratory biome.
    /// </summary>
    /// <param name="rand">The random number generator used to select between different enemy types.</param>
    /// <returns>An <see cref="Enemy"/> instance (e.g., an Acid Slime or a Flesh Golem).</returns>
    public Enemy CreateEnemy(int index, ISubject<NoiseData> NoiseEvents, ISubject<EnemyDeathData> DeathEvents)
    {
        return index % 2 == 0
            ? new Enemy("Acid Slime", "Slime", 30, 15, 0, NoiseEvents, DeathEvents, new CowardlyBehavior())
            : new Enemy("Flesh Golem", "Golem", 60, 20, 5, NoiseEvents, DeathEvents, new AggressiveBehavior());
    }

    /// <summary>
    /// Generates standard thematic loot for the player to find scattered in the laboratory.
    /// </summary>
    /// <param name="rand">The random number generator used to determine the exact item dropped.</param>
    /// <returns>An <see cref="IItem"/> representing alchemical debris or magical tools (e.g., Magic Staff or Broken Glass).</returns>
    public IItem CreateLoot(Random rand)
    {
        return rand.Next(2) == 0 ? new MagicStaff() : new BrokenGlass();
    }

    /// <summary>
    /// Constructs the unique, high-tier artifact specifically tied to the laboratory theme.
    /// </summary>
    /// <returns>An <see cref="IWeapon"/> representing the Staff of Alchemical Fire (an upgraded Magic Staff).</returns>
    public IWeapon CreateArtifact()
    {
        // Laboratory Artifact: A powerful Staff of Alchemical Fire
        return new StrongModifier(new MagicStaff());
    }
}