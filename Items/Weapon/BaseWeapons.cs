using System;

namespace AlchemyRPG;

/// <summary>
/// The abstract base class for all weapons in the game.
/// Provides shared boilerplate logic for inventory management and equipping, 
/// while enforcing the implementation of the Visitor pattern for combat and network mapping.
/// </summary>
public abstract class BaseWeapon : IWeapon
{
    public virtual int Range => 1;
    /// <summary>Gets the display name of the weapon.</summary>
    public abstract string Name { get; }

    /// <summary>Gets the base damage output of the weapon.</summary>
    public abstract int Damage { get; }

    /// <summary>Gets the luck modifier provided by the weapon. Defaults to 0.</summary>
    public virtual int LuckBonus => 0;

    /// <summary>Gets the strength modifier provided by the weapon. Defaults to 0.</summary>
    public virtual int StrengthBonus => 0;
    public virtual int WisdomBonus => 0;

    /// <summary>Indicates whether the weapon occupies both hand slots.</summary>
    public abstract bool IsTwoHanded { get; }

    /// <summary>Gets the unique identifier for this specific weapon instance.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    protected BaseWeapon()
    {
    }
    public virtual int TotalDamage => Damage;
    public virtual int TotalStrength => StrengthBonus;
    public virtual int TotalWisdom => WisdomBonus;
    public virtual int TotalLuck => LuckBonus;
    /// <summary>
    /// Handles the logic when the player picks up the weapon from the map.
    /// Transfers the weapon from the map grid into the player's backpack.
    /// </summary>
    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }

    /// <summary>
    /// Handles the logic of equipping the weapon to the appropriate hand(s).
    /// </summary>
    public void Equip(Player player, IEquipSlot slot)
    {
        if (IsTwoHanded)
            new TwoHandedSlot().Equip(this, player);
        else
            slot.Equip(this, player);
    }

    /// <summary>
    /// Accepts a combat visitor to calculate damage and defense based on the weapon's physical type.
    /// </summary>
    public abstract void Accept(IAttackVisitor visitor);

    /// <summary>
    /// Accepts a generic item visitor, forcing derived classes to explicitly resolve their type 
    /// for DTO mapping and symbol rendering.
    /// </summary>
    public abstract T Accept<T>(IItemVisitor<T> visitor);
}

/// <summary>
/// Intermediate base class for all light weapons.
/// Automatically resolves the generic visitor dispatch to the Light Weapon category.
/// </summary>
public abstract class BaseLightWeapon : BaseWeapon, ILightWeapon
{
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitLightWeapon(this);
}

/// <summary>
/// Intermediate base class for all magic weapons.
/// Automatically resolves the generic visitor dispatch to the Magic Weapon category.
/// </summary>
public abstract class BaseMagicWeapon : BaseWeapon, IMagicWeapon
{
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitMagicWeapon(this);
}

/// <summary>
/// Intermediate base class for all heavy weapons.
/// Automatically resolves the generic visitor dispatch to the Heavy Weapon category.
/// </summary>
public abstract class BaseHeavyWeapon : BaseWeapon, IHeavyWeapon
{
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitHeavyWeapon(this);
}