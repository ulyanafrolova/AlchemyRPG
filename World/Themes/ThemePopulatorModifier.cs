namespace AlchemyRPG;

/// <summary>
/// A universal modifier that uses the Abstract Factory pattern 
/// to populate the dungeon with appropriate (themed) content.
/// </summary>
public class ThemePopulatorModifier : IDungeonModifier
{
    private readonly IThemeFactory _factory;
    private readonly int _lootCount;
    private readonly int _enemyCount;
    private readonly Subject<NoiseData> _noiseEvents;
    private readonly Subject<EnemyDeathData> _deathEvents;

    public ThemePopulatorModifier(
        IThemeFactory factory,
        int lootCount,
        int enemyCount,
        Subject<NoiseData> noiseEvents,
        Subject<EnemyDeathData> deathEvents)
    {
        _factory = factory;
        _lootCount = lootCount;
        _enemyCount = enemyCount;

        _noiseEvents = noiseEvents;
        _deathEvents = deathEvents;
    }

    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        map.SpawnItemRandomly(rand, _factory.CreateArtifact());

        if (_lootCount > 0)
        {
            controls.Add($"[{Keybinds.PickUp}] Pick Up");
            controls.Add($"[{Keybinds.EquipKeysLabel}] Equip");
            controls.Add($"[{Keybinds.Drop}] Drop");

            tutorialText.Add($"- You will find scattered junk. You can pick it up [{Keybinds.PickUp}] and drop it [X] if needed.");
            tutorialText.Add($"- Weapons are hidden here. Pick them up [{Keybinds.PickUp}] and equip them [0-9] to survive.");

            for (int i = 0; i < _lootCount; i++)
            {
                map.SpawnItemRandomly(rand, _factory.CreateLoot(rand));
            }
        }

        if (_enemyCount > 0)
        {
            controls.Add($"[{Keybinds.Attack}] Attack");
            tutorialText.Add($"- Enemies lurk in the dark. Press [{Keybinds.Attack}] to engage in combat.");

            for (int i = 0; i < _enemyCount; i++)
            {
                map.SpawnEnemyRandomly(rand, _factory.CreateEnemy(i, _noiseEvents, _deathEvents));
            }
        }

        tutorialText.Add(_factory.GetWelcomeMessage());
    }
}