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
    Guid Id { get; }

    /// <summary> 
    /// Gets the display name of the item. 
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the logic when the player walks over or interacts with this item.
    /// </summary>
    /// <param name="state">The current game state, used to modify player stats or update the map.</param>
    void OnPickUp(GameState state, Player executor);

    T Accept<T>(IItemVisitor<T> visitor);
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
    int TotalDamage { get; }
    int TotalStrength { get; }
    int TotalWisdom { get; }
    int TotalLuck { get; }

    /// <summary>
    /// Attempts to equip the item to the specified hand of the player.
    /// </summary>
    /// <param name="player">The player equipping the item.</param>
    /// <param name="side">The requested hand to hold the item.</param>
    void Equip(Player player, IEquipSlot slot);
    void Accept(IAttackVisitor visitor);
}

/// <summary>
/// Defines the contract for items that can contain other items, such as slotted weapons or backpacks.
/// </summary>
public interface ISlotContainer : IInventoryItem
{
    bool AcceptItem(IInventoryItem item);
    IReadOnlyList<IInventoryItem> GetSlottedItems();
}