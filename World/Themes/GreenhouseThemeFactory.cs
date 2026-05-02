namespace AlchemyRPG;

/// <summary>
/// A concrete implementation of the <see cref="IThemeFactory"/> that generates an "Overgrown Greenhouse" biome.
/// This theme is characterized by a dense, maze-like layout, plant-based mutated enemies, 
/// and specialized thematic loot.
/// </summary>
public class GreenhouseThemeFactory : IThemeFactory
{
    /// <summary>
    /// Retrieves the atmospheric narrative text to display when the player enters this specific theme.
    /// </summary>
    /// <returns>A descriptive string setting the mood for the greenhouse.</returns>
    public string GetWelcomeMessage() => "\nThe smell of rot and exotic herbs fills the damp air...";

    /// <summary>
    /// Dictates the architectural layout of the greenhouse dungeon.
    /// Uses a dense block of walls carved out entirely by twisting corridors to simulate a chaotic, overgrown maze.
    /// </summary>
    /// <param name="builder">The dungeon builder instance to apply the layout modifiers to.</param>
    public void ConfigureBuilder(IDungeonBuilder builder)
    {
        builder.CreateFilled(40, 20).ApplyModifier(new CorridorsModifier());
    }

    /// <summary>
    /// Creates a theme-appropriate enemy for the greenhouse biome.
    /// </summary>
    /// <param name="rand">The random number generator used to select between different enemy types.</param>
    /// <returns>An <see cref="Enemy"/> instance (e.g., a Mutated Mandrake or a Carnivorous Plant).</returns>
    public Enemy CreateEnemy(int index, Subject<NoiseData> noiseEvents, Subject<EnemyDeathData> deathEvents)
    {
        return index % 2 == 0
            ? new Enemy("Mutated Mandrake", "Plant", 25, 10, 1, noiseEvents, deathEvents, new CowardlyBehavior())
            : new Enemy("Carnivorous Plant", "Plant", 40, 18, 3, noiseEvents, deathEvents, new AggressiveBehavior());
    }

    /// <summary>
    /// Generates standard thematic loot for the player to find scattered in the greenhouse.
    /// </summary>
    /// <param name="rand">The random number generator used to determine the exact item dropped.</param>
    /// <returns>An <see cref="IItem"/> representing basic loot (e.g., Dagger or Old Bone).</returns>
    public IItem CreateLoot(Random rand)
    {
        return rand.Next(2) == 0 ? new Dagger() : new OldBone();
    }

    /// <summary>
    /// Constructs the unique, high-tier artifact specifically tied to the greenhouse theme.
    /// </summary>
    /// <returns>An <see cref="IWeapon"/> representing the artifact. In this case, a heavily upgraded Dagger.</returns>
    public IWeapon CreateArtifact()
    {
        var artifact = new Dagger();
        // Applying the StrongModifier twice to create a highly potent artifact
        return new StrongModifier(new StrongModifier(artifact));
    }
}