using System.Collections.Generic;
namespace AlchemyRPG;

public class Player(int startX, int startY)
{
    public int X { get; private set; } = startX;
    public int Y { get; private set; } = startY;
    public int Strength { get; private set; } = 10;
    public int Dexterity { get; private set; } = 10;
    public int Health { get; private set; } = 100;
    public int Luck { get; private set; } = 5;
    public int Aggression { get; private set; } = 5;
    public int Wisdom { get; private set; } = 10;
    public int Coins { get; set; } = 0;
    public int Gold { get; set; } = 0;
    public List<IInventoryItem> Backpack { get; private set; } = [];
    public IInventoryItem? LeftHand { get; private set; }
    public IInventoryItem? RightHand { get; private set; }

    public string LogMessage { get; set; } = "";

    public void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }
    public void TryEquipFromBackpack(int index, HandSide side)
    {
        if (index >= 0 && index < Backpack.Count)
        {
            var item = Backpack[index];
            item.Equip(this, side);
        }
    }
    public void EquipLeftHand(IInventoryItem item)
    {
        if (RightHand != null && RightHand.IsTwoHanded)
        {
            RightHand = null;
        }
        LeftHand = item;
        if (RightHand == item) RightHand = null;

        LogMessage = $"Equipped to LEFT hand: {item.Name}";
    }

    public void EquipRightHand(IInventoryItem item)
    {
        if (LeftHand != null && LeftHand.IsTwoHanded)
        {
            LeftHand = null;
        }

        RightHand = item;
        if (LeftHand == item) LeftHand = null;

        LogMessage = $"Equipped to RIGHT hand: {item.Name}";
    }
    public void EquipTwoHanded(IInventoryItem item)
    {
        LeftHand = item;
        RightHand = item;
        LogMessage = $"Equipped two-handed: {item.Name}";
    }
    public IInventoryItem? DropItem(int index)
    {
        if (index >= 0 && index < Backpack.Count)
        {
            var item = Backpack[index];
            Backpack.RemoveAt(index);
            LogMessage = $"Dropped: {item.Name}";
            if (LeftHand == item) LeftHand = null;
            if (RightHand == item) RightHand = null;
            return item;
        }
        return null;
    }
}