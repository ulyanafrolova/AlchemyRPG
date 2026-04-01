namespace AlchemyRPG;

/// <summary>
/// A base abstract class for all weapons.
/// It implements picking up and equipping logic 
/// </summary>
public abstract class BaseWeapon : IWeapon
{
    public abstract string Name { get; }
    public char Symbol => Tiles.Weapon;
    public abstract int Damage { get; }
    public virtual int LuckBonus => 0;
    public abstract bool IsTwoHanded { get; }

    /// <summary>
    /// Moves the weapon from the map floor into the player's backpack.
    /// </summary>
    public void OnPickUp(GameState state)
    {
        state.Player.Backpack.Add(this);
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
        state.Log = $"Picked up weapon: {Name}";
    }
    /// <summary>
    /// Handles the rules for putting the weapon into the player's hands.
    /// </summary>
    public void Equip(Player player, HandSide side)
    {
        if (IsTwoHanded) player.EquipTwoHanded(this);
        else if (side == HandSide.Left) player.EquipLeftHand(this);
        else player.EquipRightHand(this);
    }
    /// <summary>
    /// We force every specific weapon class to implement this so it can route the attack 
    /// to the correct mathematical formula without using 'switch' statements.
    /// </summary>
    public abstract void Accept(IAttackVisitor visitor, IInventoryItem context);
}

/// <summary>
/// Represents a lightweight, single-handed weapon with low damage output.
/// </summary>
public class Dagger : BaseWeapon, ILightWeapon
{
    public override string Name => "Dagger";
    public override int Damage => 5;
    public override bool IsTwoHanded => false;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitLightWeapon((IWeapon)context);
}

/// <summary>
/// Represents a sword weapon that can be used in combat and supports heavy weapon interactions.
/// </summary>
public class Sword : BaseWeapon, IHeavyWeapon
{
    public override string Name => "Sword";
    public override int Damage => 10;
    public override bool IsTwoHanded => false;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitHeavyWeapon((IWeapon)context);
}

/// <summary>
/// Represents a two-handed axe weapon that deals heavy damage and requires both hands
/// </summary>
public class TwoHandedAxe : BaseWeapon, IHeavyWeapon
{
    public override string Name => "Two-Handed Axe";
    public override int Damage => 25;
    public override bool IsTwoHanded => true;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitHeavyWeapon((IWeapon)context);
}

/// <summary>
/// Represents a magical staff weapon that can be used to perform magic-based attacks.
/// </summary>
public class MagicStaff : BaseWeapon, IMagicWeapon
{
    public override string Name => "Magic Staff";
    public override int Damage => 8;
    public override bool IsTwoHanded => true;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitMagicWeapon((IWeapon)context);
}