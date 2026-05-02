namespace AlchemyRPG;

/// <summary>
/// Defines a contract for creating themed game elements such as welcome messages, loot items, artifacts, and enemies
/// </summary>
public interface IThemeFactory
{
    /// <summary>
    /// Retrieves a welcome message for display to the user.
    /// </summary>
    /// <returns>A string containing the welcome message. The value may be empty if no message is available.</returns>
    string GetWelcomeMessage();

    /// <summary>
    /// Generates a new loot item using the specified random number generator.
    /// </summary>
    /// <param name="rand">The random number generator used to determine the properties of the created loot item. Cannot be null.</param>
    /// <returns>An object that implements the IItem interface representing the generated loot item.</returns>
    IItem CreateLoot(Random rand);

    /// <summary>
    /// Creates a new weapon artifact instance.
    /// </summary>
    /// <returns>An object that implements the IWeapon interface representing the newly created artifact.</returns>
    IWeapon CreateArtifact();

    /// <summary>
    /// Creates a new enemy instance using the specified random number generator.
    /// </summary>
    /// <param name="rand">The random number generator used to determine the properties of the created enemy. Cannot be null.</param>
    /// <returns>An object that implements the IEnemy interface representing the generated enemy.</returns>
    Enemy CreateEnemy(int index, ISubject<NoiseData> NoiseEvents, ISubject<EnemyDeathData> DeathEvents);
     void ConfigureBuilder(IDungeonBuilder builder);
}