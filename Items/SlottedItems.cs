using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents a non-weapon inventory item that provides passive attribute bonuses when held or equipped.
/// </summary>
/// <remarks>PassiveItem serves as a base class for items that do not require active use or equipping in both
/// hands. Derived classes define specific attribute bonuses and item names. Passive items are typically added to a
/// player's backpack and can be equipped to provide their effects. This class implements IInventoryItem and provides
/// default implementations for common inventory behaviors.</remarks>
public abstract class PassiveItem : IInventoryItem
{
    public abstract string Name { get; }
    public Guid Id { get; } = Guid.NewGuid();

    public bool IsTwoHanded => false;

    public virtual int DamageBonus => 0;
    public virtual int StrengthBonus => 0;
    public virtual int WisdomBonus => 0;
    public virtual int LuckBonus => 0;

    public int TotalDamage => DamageBonus;
    public int TotalStrength => StrengthBonus;
    public int TotalWisdom => WisdomBonus;
    public int TotalLuck => LuckBonus;

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }

    public void Equip(Player player, IEquipSlot slot)
    {
        slot.Equip(this, player);
    }

    public void Accept(IAttackVisitor visitor) => visitor.VisitNonWeapon();
    public abstract T Accept<T>(IItemVisitor<T> visitor);
}

public class StrengthStone : PassiveItem
{
    public override string Name => "Stone of Strength";
    public override int StrengthBonus => 2;
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitPassive(this);
}

public class WisdomStone : PassiveItem
{
    public override string Name => "Stone of Wisdom";
    public override int WisdomBonus => 2;
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitPassive(this);
}

public class LuckStone : PassiveItem
{
    public override string Name => "Stone of Luck";
    public override int LuckBonus => 2;
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitPassive(this);
}

public class ItemHolder : ISlotContainer
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsTwoHanded => false;

    private readonly int _maxSlots;
    private readonly List<IInventoryItem> _slots = new();

    public ItemHolder(int maxSlots)
    {
        _maxSlots = maxSlots;
    }

    public string Name => $"Holder ({_slots.Count}/{_maxSlots})";

    public int TotalDamage => _slots.Sum(i => i.TotalDamage);
    public int TotalStrength => _slots.Sum(i => i.TotalStrength);
    public int TotalWisdom => _slots.Sum(i => i.TotalWisdom);
    public int TotalLuck => _slots.Sum(i => i.TotalLuck);

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }

    public void Equip(Player player, IEquipSlot slot)
    {
        slot.Equip(this, player);
    }

    public void Accept(IAttackVisitor visitor) => visitor.VisitNonWeapon();
    public T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitHolder(this);

    public bool AcceptItem(IInventoryItem item)
    {
        if (_slots.Count >= _maxSlots) return false;
        var validator = new AllowedInSlotVisitor();
        if (!item.Accept(validator)) return false;

        var cycleChecker = new ContainsItemVisitor(this.Id);
        if (item.Id == this.Id || item.Accept(cycleChecker)) return false;

        _slots.Add(item);
        return true;
    }

    public IReadOnlyList<IInventoryItem> GetSlottedItems() => _slots.AsReadOnly();
}