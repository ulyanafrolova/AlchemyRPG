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
    /// Strategy 1: "Teren lochu"
    /// Generates a complex maze featuring a central hall, additional scattered rooms, 
    /// corridors connecting them, and random loot.
    /// </summary>
    public void ConstructStandardDungeon()
    {
        // Step 1: Start with a solid block of walls as the base
        _builder.CreateFilled(40, 20)
                // Step 2: Carve out a large central hall
                .ApplyModifier(new CentralRoomModifier(8, 6))
                // Step 3: Carve 5 additional random rectangular rooms
                .ApplyModifier(new RoomsModifier(5))
                // Step 4: Generate a maze of corridors to connect open spaces
                .ApplyModifier(new CorridorsModifier())
                // Step 5: Place 5 pieces of unusable junk 
                .ApplyModifier(new JunkItemsModifier(5))
                // Step 6: Place 3 usable weapons 
                .ApplyModifier(new WeaponsModifier(7))
                // Step 7: Enemies
                .ApplyModifier(new EnemiesModifier(5));
    }

    /// <summary>
    /// Strategy 2: "Arena".
    /// Generates a wide open space with no obstacles and plenty of weapons.
    /// </summary>
    public void ConstructArena()
    {
        // Step 1: Start with an empty room 
        _builder.CreateEmpty(40, 20)
                // Step 2: Place weapons directly onto the open floor
                .ApplyModifier(new WeaponsModifier(5));
    }
}
