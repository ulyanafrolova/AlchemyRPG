namespace AlchemyRPG;

/// <summary>
/// Defines a contract for an equipment slot that can equip an inventory item to a player.
/// </summary>
/// <remarks>Implementations of this interface represent specific equipment slots, such as head, body, or
/// accessory slots, and determine how items are equipped to a player. The behavior of equipping may vary depending on
/// the slot type and item compatibility.</remarks>
public interface IEquipSlot
{
    void Equip(IInventoryItem item, Player player);
}

public class LeftHandSlot : IEquipSlot
{
    public void Equip(IInventoryItem item, Player player) => player.EquipLeftHand(item);
}

public class RightHandSlot : IEquipSlot
{
    public void Equip(IInventoryItem item, Player player) => player.EquipRightHand(item);
}

public class TwoHandedSlot : IEquipSlot
{
    public void Equip(IInventoryItem item, Player player) => player.EquipTwoHanded(item);
}