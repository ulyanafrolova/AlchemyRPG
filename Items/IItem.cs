namespace AlchemyRPG;

/// <summary>
/// Specifies the physical hand used to equip an item.
/// </summary>
public enum HandSide { Left, Right }

/// <summary>
/// Defines the base contract for any object that can be placed on the game map and interacted with.
/// </summary>
public interface IItem
{
    /// <summary> 
    /// Gets the display name of the item. 
    /// </summary>
    string Name { get; }

    /// <summary> 
    /// Gets the character symbol used to represent this item on the map grid. 
    /// </summary>
    char Symbol { get; }

    /// <summary>
    /// Executes the logic when the player walks over or interacts with this item.
    /// </summary>
    /// <param name="state">The current game state, used to modify player stats or update the map.</param>
    void OnPickUp(GameState state);
}

/// <summary>
/// Extends the base item contract for objects that can be stored in a player's backpack 
/// and equipped in their hands (e.g., weapons, junk).
/// </summary>
public interface IInventoryItem : IItem
{
    /// <summary> 
    /// Indicates whether the item requires both hands to be equipped. 
    /// </summary>
    bool IsTwoHanded { get; }

    int LuckBonus { get; }

    /// <summary>
    /// Attempts to equip the item to the specified hand of the player.
    /// </summary>
    /// <param name="player">The player equipping the item.</param>
    /// <param name="side">The requested hand to hold the item.</param>
    void Equip(Player player, HandSide side);

    /// <summary>
    /// Accepts a visitor that performs an operation on this inventory item within the specified context.
    /// </summary>
    /// <param name="visitor">The visitor that defines the operation to perform on the inventory item</param>
    /// <param name="context">The context in which the visitor operates</param>
    void Accept(IAttackVisitor visitor);
}