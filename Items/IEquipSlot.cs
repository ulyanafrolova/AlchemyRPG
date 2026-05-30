namespace AlchemyRPG;
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