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

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
        state.EventLog.Push($"Picked up junk: {Name}");
        state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} picked up {Name}"));
    }

    public void Equip(Player player, HandSide side)
    {
        if (side == HandSide.Left) player.EquipLeftHand(this);
        else player.EquipRightHand(this);
    }

    public T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitJunk(this);
    public void Accept(IAttackVisitor visitor) => visitor.VisitNonWeapon();
}

public class Skull : Junk { public override string Name => "Skull"; }
public class OldBone : Junk { public override string Name => "Old Bone"; }
public class BrokenGlass : Junk { public override string Name => "Broken Glass"; }
