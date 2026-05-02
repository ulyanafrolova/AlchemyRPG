namespace AlchemyRPG;

/// <summary>
/// The base abstract class for useless items that occupy backpack space 
/// but cannot be equipped or utilized in combat.
/// </summary>
public abstract class Junk : IInventoryItem
{
    public abstract string Name { get; }
    public int LuckBonus => 0;
    public virtual int NoiseRange => 0;
    public char Symbol => Tiles.Unknown;
    public bool IsTwoHanded => false;
    public void OnPickUp(GameState state)
    {
        state.Player.Backpack.Add(this);
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
        GameLogger.Instance.Log(LogType.Loot, $"{state.Player.Name} picked up {Name}.");
        state.Log = $"Picked up junk: {Name}";
    }
    public void Equip(Player player, HandSide side)
    {
        if (side == HandSide.Left) player.EquipLeftHand(this);
        else player.EquipRightHand(this);
    }

    public void Accept(IAttackVisitor visitor) => visitor.VisitNonWeapon();
}

/// <summary> 
/// A piece of junk representing a skull.
/// </summary>
public class Skull : Junk { public override string Name => "Skull"; }

/// <summary> 
/// A piece of junk representing an old bone. 
/// </summary>
public class OldBone : Junk { public override string Name => "Old Bone"; }

/// <summary> 
/// A piece of junk representing broken glass. 
/// </summary>
public class BrokenGlass : Junk { public override string Name => "Broken Glass"; }
