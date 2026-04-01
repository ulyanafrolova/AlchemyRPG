namespace AlchemyRPG;

/// <summary>
/// Carves pathways into the current grid to connect isolated areas.
/// It delegates the actual math and pathfinding to the Labyrinth class 
/// /// </summary>
public class CorridorsModifier : IDungeonModifier
{
    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- You must navigate through a twisting maze of corridors.");
        var maze = Labyrinth.Generate(map.Width, map.Height);
        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
                if (maze[y, x] == Tiles.Floor) map.Grid[y, x] = Tiles.Floor;
    }
}

/// <summary>
/// Carves rectangular open spaces into the map.
/// </summary>
public class RoomsModifier : IDungeonModifier
{
    private readonly int _numberOfRooms;

    public RoomsModifier(int count) => _numberOfRooms = count;

    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- Explore various hidden rooms scattered across the area.");
        for (int i = 0; i < _numberOfRooms; i++)
        {
            int w = rand.Next(3, 8), h = rand.Next(3, 6);
            int x = rand.Next(1, map.Width - w - 1), y = rand.Next(1, map.Height - h - 1);
            for (int ry = y; ry < y + h; ry++)
                for (int rx = x; rx < x + w; rx++)
                    map.Grid[ry, rx] = Tiles.Floor;
        }
    }
}

/// <summary>
/// Carves a specific main room in the geometric center of the map.
/// </summary>
public class CentralRoomModifier : IDungeonModifier
{
    private readonly int _roomWidth;
    private readonly int _roomHeight;

    public CentralRoomModifier(int width, int height)
    {
        _roomWidth = width;
        _roomHeight = height;
    }

    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        tutorialText.Add("- A massive central hall lies in the middle of the dungeon.");
        int startX = (map.Width - _roomWidth) / 2, startY = (map.Height - _roomHeight) / 2;
        for (int y = startY; y < startY + _roomHeight; y++)
            for (int x = startX; x < startX + _roomWidth; x++)
                map.Grid[y, x] = Tiles.Floor;
    }
}

/// <summary>
/// Adds junk items into the world state.
/// </summary>
public class JunkItemsModifier : IDungeonModifier
{
    private readonly int _count;

    public JunkItemsModifier(int count) => _count = count;

    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        if (_count <= 0) return;
        controls.Add($"[{Keybinds.PickUp}] Pick Up"); controls.Add($"[{Keybinds.Drop}] Drop");
        tutorialText.Add($"- You will find scattered junk. You can pick it up [{Keybinds.PickUp}] and drop it [{Keybinds.Drop}] if needed.");

        for (int i = 0; i < _count; i++)
            map.SpawnItemRandomly(rand, rand.Next(3) switch { 0 => new Skull(), 1 => new OldBone(), _ => new BrokenGlass() });
    }
}

/// <summary>
/// Adds functional, equippable items into the world state.
/// </summary>
public class WeaponsModifier : IDungeonModifier
{
    private readonly int _count;

    public WeaponsModifier(int count) => _count = count;

    public void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand)
    {
        if (_count <= 0) return;
        controls.Add($"[{Keybinds.PickUp}] Pick Up"); controls.Add($"[{Keybinds.EquipKeysLabel}] Equip");
        tutorialText.Add($"- Weapons are hidden here. Pick them up [{Keybinds.PickUp}] and equip them [{Keybinds.EquipKeysLabel}] to survive.");

        for (int i = 0; i < _count; i++)
        {
            IWeapon weapon = rand.Next(4) switch
            {
                0 => new Dagger(),
                1 => new Sword(),
                2 => new TwoHandedAxe(),
                _ => new MagicStaff()
            };
            if (rand.Next(100) < 50) 
            {
                weapon = new StrongModifier(weapon);
            }
            if (rand.Next(100) < 50) 
            {
                weapon = new UnluckyModifier(weapon);
            }
            map.SpawnItemRandomly(rand, weapon);
        }
    }
}