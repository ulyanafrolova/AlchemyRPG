namespace AlchemyRPG;

/// <summary>
/// Represents the main character controlled by the user.
/// Manages the player's position, role-playing statistics, inventory, and equipment.
/// </summary>
/// <param name="startX">The initial horizontal position of the player on the map.</param>
/// <param name="startY">The initial vertical position of the player on the map.</param>
public class Player: Entity
{
    // RPG Statistics

    /// <summary> 
    /// Gets the player's physical strength. 
    /// </summary>
    public int Strength { get; private set; } = 10;

    /// <summary> 
    /// Gets the player's agility and speed. 
    /// </summary>
    public int Dexterity { get; private set; } = 10;

    /// <summary> 
    /// Gets the player's luck. 
    /// </summary>
    public int Luck { get; private set; } = 5;

    public int TotalLuck
    {
        get
        {
            int bonus = 0;
            if (RightHand is IWeapon rw) bonus += rw.LuckBonus;
            if (LeftHand is IWeapon lw) bonus += lw.LuckBonus;
            return Luck + bonus;
        }
    }

    /// <summary> 
    /// Gets the player's aggression level. 
    /// </summary>
    public int Aggression { get; private set; } = 5;

    /// <summary> 
    /// Gets the player's wisdom. 
    /// </summary>
    public int Wisdom { get; private set; } = 10;

    // Currency & Inventory

    /// <summary> 
    /// Gets or sets the amount of standard coin currency the player holds. 
    /// </summary>
    public int Coins { get; set; } = 0;

    /// <summary> 
    /// Gets or sets the amount of premium gold currency the player holds. 
    /// </summary>
    public int Gold { get; set; } = 0;

    /// <summary> 
    /// A collection of items currently stored in the player's inventory. 
    /// </summary>
    public List<IInventoryItem> Backpack { get; private set; } = [];

    /// <summary> 
    /// The item currently held in the player's left hand. Null if empty. 
    /// </summary>
    public IInventoryItem? LeftHand { get; private set; }

    /// <summary> 
    /// The item currently held in the player's right hand. Null if empty.
    /// </summary>
    public IInventoryItem? RightHand { get; private set; }

    /// <summary> 
    /// A temporary message indicating the result of the player's latest action. 
    /// (e.g., picking up an item, equipping a weapon)
    /// </summary>
    public string LogMessage { get; set; } = "";

    public Player(string name, int startX, int startY) : base(name, Tiles.Player, 100)
    {
        X = startX;
        Y = startY;
    }

    /// <summary>
    /// Updates the player's coordinates on the map.
    /// </summary>
    /// <param name="dx">The change in the X direction (horizontal).</param>
    /// <param name="dy">The change in the Y direction (vertical).</param>
    public void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }

    /// <summary>
    /// Attempts to equip an item from the backpack to the specified hand.
    /// Delegates the actual equipping logic to the item itself.
    /// </summary>
    /// <param name="index">The index of the item in the backpack.</param>
    /// <param name="side">The requested hand to equip the item to.</param>
    public void TryEquipFromBackpack(int index, HandSide side)
    {
        if (index >= 0 && index < Backpack.Count)
        {
            var item = Backpack[index];
            item.Equip(this, side);
        }
    }

    /// <summary>
    /// Equips a given item strictly to the left hand.
    /// Automatically handles un-equipping two-handed weapons from the right hand 
    /// to prevent holding a two-handed weapon in one hand.
    /// </summary>
    /// <param name="item">The item to be equipped.</param>
    public void EquipLeftHand(IInventoryItem item)
    {
        if (RightHand != null && RightHand.IsTwoHanded)
        {
            RightHand = null;
        }
        LeftHand = item;
        if (RightHand == item) RightHand = null;

        GameLogger.Instance.Log(LogType.Loot, $"{Name} equipped to LEFT hand: {item.Name}");
    }

    /// <summary>
    /// Equips a given item strictly to the right hand.
    /// Automatically handles un-equipping two-handed weapons from the left hand.
    /// </summary>
    /// <param name="item">The item to be equipped.</param>
    public void EquipRightHand(IInventoryItem item)
    {
        if (LeftHand != null && LeftHand.IsTwoHanded)
        {
            LeftHand = null;
        }

        RightHand = item;
        if (LeftHand == item) LeftHand = null;

        GameLogger.Instance.Log(LogType.Loot, $"{Name} equipped to RIGHT hand: {item.Name}");
    }

    /// <summary>
    /// Equips a two-handed weapon, occupying both the left and right hand slots simultaneously.
    /// </summary>
    /// <param name="item">The two-handed item to be equipped.</param>
    public void EquipTwoHanded(IInventoryItem item)
    {
        LeftHand = item;
        RightHand = item;
        GameLogger.Instance.Log(LogType.Loot, $"{Name} equipped two-handed: {item.Name}");
    }

    /// <summary>
    /// Removes an item from the player's backpack and unequips it if currently held.
    /// </summary>
    /// <param name="index">The inventory index of the item to drop.</param>
    /// <returns>The dropped item, or null if the index was invalid.</returns>
    public IInventoryItem? DropItem(int index)
    {
        if (index >= 0 && index < Backpack.Count)
        {
            var item = Backpack[index];
            Backpack.RemoveAt(index);
            GameLogger.Instance.Log(LogType.Loot, $"{Name} dropped: {item.Name}");
            if (LeftHand == item) LeftHand = null;
            if (RightHand == item) RightHand = null;
            return item;
        }
        return null;
    }
}