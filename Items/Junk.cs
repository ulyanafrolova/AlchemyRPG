using System;

namespace AlchemyRPG;

/// <summary>
/// The base abstract class for useless items that occupy backpack space 
/// but cannot be equipped or utilized in combat.
/// </summary>
public abstract class Junk : IInventoryItem
{
    public abstract string Name { get; }
    public Guid Id { get; } = Guid.NewGuid();
    public int LuckBonus => 0;
    public bool IsTwoHanded => false;
    public int TotalDamage => 0;
    public int TotalStrength => 0;
    public int TotalWisdom => 0;
    public int TotalLuck => 0;
    
    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
        state.EventLog.Push($"Picked up junk: {Name}");
        state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} picked up {Name}"));
    }

    public void Equip(Player player, IEquipSlot slot)
    {
        slot.Equip(this, player);
    }

    public T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitJunk(this);
    public void Accept(IAttackVisitor visitor) => visitor.VisitNonWeapon();
}

public class Skull : Junk { public override string Name => "Skull"; }
public class OldBone : Junk { public override string Name => "Old Bone"; }
public class BrokenGlass : Junk { public override string Name => "Broken Glass"; }