namespace AlchemyRPG;

/// <summary>
/// Implements the <see cref="IDungeonModifier"/> interface to populate the map with enemy entities
/// </summary>
public class EnemiesModifier : IDungeonModifier
{
    private readonly int _count;
    /// <summary>
    /// Initializes a new instance of the EnemiesModifier.
    /// </summary>
    /// <param name="count">The exact number of enemies to spawn on the current map.</param>
    public EnemiesModifier(int count) => _count = count;

    /// <summary>
    /// Executes the modifier's logic upon the map state.
    /// </summary>
    /// <param name="map">The map instance being modified.</param>
    /// <param name="controls">The hash set storing unique UI keybind hints.</param>
    /// <param name="tutorialText">The list storing sequential tutorial lines.</param>
    /// <param name="rand">The pseudo-random number generator instance provided by the builder.</param>
    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        if (_count <= 0) return;

        controls.Add($"[{Keybinds.Attack}] Attack");
        tutorialText.Add($"- Enemies lurk in the dark. Press [{Keybinds.Attack}] to engage in combat.");

        for (int i = 0; i < _count; i++)
        {
            Enemy enemy = rand.Next(2) == 0
                ? new Enemy("Goblin", health: 30, attackDamage: 15, armor: 2)
                : new Enemy("Orc", health: 50, attackDamage: 25, armor: 5);

            map.SpawnItemRandomly(rand, enemy);
        }
    }
}