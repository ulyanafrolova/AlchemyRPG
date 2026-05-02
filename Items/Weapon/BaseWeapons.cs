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
    public virtual int StrengthBonus => 0;
    public virtual int NoiseRange => 3;
    public abstract bool IsTwoHanded { get; }

    /// <summary>
    /// Moves the weapon from the map floor into the player's backpack.
    /// </summary>
    public void OnPickUp(GameState state)
    {
        state.Player.Backpack.Add(this);
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
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
    public abstract void Accept(IAttackVisitor visitor);
}

/// <summary>
/// Intermediate base class for all light weapons.
/// Encapsulates shared behavior so child classes stay clean (DRY principle).
/// </summary>
public abstract class BaseLightWeapon : BaseWeapon, ILightWeapon
{
    /// <summary>
    /// All light weapons are nearly silent by default.
    /// Child classes no longer need to override this!
    /// </summary>
    public override int NoiseRange => 1;
}

/// <summary>
/// Intermediate base class for all magic weapons.
/// </summary>
public abstract class BaseMagicWeapon : BaseWeapon, IMagicWeapon
{
    public override int NoiseRange => 3;
}

/// <summary>
/// Intermediate base class for all heavy weapons.
/// </summary>
public abstract class BaseHeavyWeapon : BaseWeapon, IHeavyWeapon
{
    /// <summary>
    /// All heavy weapons are loud by default.
    /// </summary>
    public override int NoiseRange => 5;
}