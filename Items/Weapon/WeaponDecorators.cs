using System;

namespace AlchemyRPG;

/// <summary>
/// Serves as the abstract base class for the Decorator design pattern applied to weapons.
/// Allows dynamic attachment of stat modifiers or magical effects to existing weapon instances at runtime.
/// </summary>
public abstract class WeaponDecorator : IWeapon
{
    /// <summary>
    /// The wrapped weapon instance being decorated.
    /// </summary>
    protected readonly IWeapon _innerWeapon;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponDecorator"/> class.
    /// </summary>
    /// <param name="innerWeapon">The core weapon to wrap and augment.</param>
    public WeaponDecorator(IWeapon innerWeapon)
    {
        _innerWeapon = innerWeapon;
    }

    public Guid Id => _innerWeapon.Id;
    public virtual string Name => _innerWeapon.Name;
    public virtual int Damage => _innerWeapon.Damage;
    public virtual int LuckBonus => _innerWeapon.LuckBonus;
    public bool IsTwoHanded => _innerWeapon.IsTwoHanded;

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }

    public T Accept<T>(IItemVisitor<T> visitor) => _innerWeapon.Accept(visitor);

    public void Equip(Player player, HandSide side)
    {
        if (IsTwoHanded) player.EquipTwoHanded(this);
        else if (side == HandSide.Left) player.EquipLeftHand(this);
        else player.EquipRightHand(this);
    }

    /// <summary>
    /// Accepts a combat visitor. Uses a specialized proxy to implement flawless Double Dispatch,
    /// ensuring the visitor recognizes the core weapon type while calculating damage using the Decorator's stats.
    /// </summary>
    public virtual void Accept(IAttackVisitor visitor)
    {
        var proxy = new DecoratorVisitorProxy(visitor, this);
        _innerWeapon.Accept(proxy);
    }

    /// <summary>
    /// A private proxy class that intercepts the double dispatch process.
    /// It captures the correct type routing from the wrapped <c>_innerWeapon</c>, 
    /// but explicitly passes the outer <c>_decorator</c> to the real visitor for stat extraction.
    /// </summary>
    private class DecoratorVisitorProxy : IAttackVisitor
    {
        private readonly IAttackVisitor _realVisitor;
        private readonly IWeapon _decorator;

        public DecoratorVisitorProxy(IAttackVisitor realVisitor, IWeapon decorator)
        {
            _realVisitor = realVisitor;
            _decorator = decorator;
        }

        public void VisitHeavyWeapon(IWeapon weapon) => _realVisitor.VisitHeavyWeapon(_decorator);
        public void VisitLightWeapon(IWeapon weapon) => _realVisitor.VisitLightWeapon(_decorator);
        public void VisitMagicWeapon(IWeapon weapon) => _realVisitor.VisitMagicWeapon(_decorator);
        public void VisitNonWeapon() => _realVisitor.VisitNonWeapon();
    }
}

/// <summary>
/// A concrete decorator that permanently increases the damage property of the wrapped weapon.
/// </summary>
public class StrongModifier : WeaponDecorator
{
    public StrongModifier(IWeapon inner) : base(inner) { }

    public override string Name => _innerWeapon.Name + " (Strong)";
    public override int Damage => _innerWeapon.Damage + 5;
}

/// <summary>
/// A concrete decorator that decreases the luck bonus of the wrapped weapon.
/// </summary>
public class UnluckyModifier : WeaponDecorator
{
    public UnluckyModifier(IWeapon inner) : base(inner) { }

    public override string Name => _innerWeapon.Name + " (Unlucky)";
    public override int LuckBonus => _innerWeapon.LuckBonus - 5;
}