namespace AlchemyRPG;

/// <summary>
/// Serves as the abstract base class for the Decorator design pattern.
/// Weapon classes do not know about their active effects.
/// </summary>
public abstract class WeaponDecorator : IWeapon
{
    /// <summary>
    /// The internal weapon instance being decorated. 
    /// Protected so concrete modifiers can access its base properties for calculations.
    /// </summary>
    protected readonly IWeapon _innerWeapon;
    /// <summary>
    /// Initializes a new instance of the WeaponDecorator.
    /// </summary>
    /// <param name="innerWeapon">The IWeapon component to be extended.</param>
    public WeaponDecorator(IWeapon innerWeapon)
    {
        _innerWeapon = innerWeapon;
    }
    public virtual int NoiseRange => _innerWeapon.NoiseRange;
    public virtual string Name => _innerWeapon.Name;
    public virtual int Damage => _innerWeapon.Damage;
    public virtual int LuckBonus => _innerWeapon.LuckBonus;
    public char Symbol => _innerWeapon.Symbol;
    public bool IsTwoHanded => _innerWeapon.IsTwoHanded;

    /// <summary>
    /// Handles the pickup mechanic.
    /// We pass 'this' (the decorated object) into the backpack rather than '_innerWeapon'.
    /// This ensures the modifiers stay when the item is added to the inventory.
    /// </summary>
    public void OnPickUp(GameState state)
    {
        state.Player.Backpack.Add(this);
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
        state.Log = $"Picked up weapon: {Name}";
    }

    /// <summary>
    /// Equips the decorated weapon to the appropriate hand.
    /// </summary>
    public void Equip(Player player, HandSide side)
    {
        if (IsTwoHanded) player.EquipTwoHanded(this);
        else if (side == HandSide.Left) player.EquipLeftHand(this);
        else player.EquipRightHand(this);
    }
    public void Accept(IAttackVisitor visitor, IInventoryItem context)
    {
        _innerWeapon.Accept(visitor, context);
    }
}

/// <summary>
/// A concrete decorator that changes the damage property of the weapon
/// </summary>
public class StrongModifier : WeaponDecorator
{
    public StrongModifier(IWeapon inner) : base(inner) { }

    public override string Name => _innerWeapon.Name + " (Strong)";
    public override int Damage => _innerWeapon.Damage + 5;
}

/// <summary>
/// A concrete decorator that changes a player attribute via the weapon.
/// </summary>
public class UnluckyModifier : WeaponDecorator
{
    public UnluckyModifier(IWeapon inner) : base(inner) { }

    public override string Name => _innerWeapon.Name + " (Unlucky)";
    public override int LuckBonus => _innerWeapon.LuckBonus - 5;
}