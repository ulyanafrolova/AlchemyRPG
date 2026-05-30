using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Defines the base structural component for the Decorator design pattern applied to weapons.
/// </summary>
/// <remarks>
/// Allows dynamic, runtime augmentation of weapon properties without subclassing. 
/// By default, all calls are transparently delegated to the wrapped <see cref="_innerWeapon"/> 
/// unless overridden by a concrete modifier.
/// </remarks>
public abstract class WeaponDecorator : IWeapon
{
    public virtual int Range => _innerWeapon.Range;
    protected readonly IWeapon _innerWeapon;

    /// <summary>
    /// Initializes a new instance of the decorator, wrapping an existing weapon or another decorator.
    /// </summary>
    /// <param name="innerWeapon">The component to wrap.</param>
    public WeaponDecorator(IWeapon innerWeapon)
    {
        _innerWeapon = innerWeapon;
    }

    public Guid Id => _innerWeapon.Id;
    public virtual string Name => _innerWeapon.Name;
    public virtual int Damage => _innerWeapon.Damage;
    public bool IsTwoHanded => _innerWeapon.IsTwoHanded;

    public virtual int TotalDamage => _innerWeapon.TotalDamage;
    public virtual int TotalStrength => _innerWeapon.TotalStrength;
    public virtual int TotalWisdom => _innerWeapon.TotalWisdom;
    public virtual int TotalLuck => _innerWeapon.TotalLuck;

    /// <summary>
    /// Delegates the pickup lifecycle event to the innermost weapon implementation.
    /// </summary>
    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddToBackpack(this);
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }

    /// <summary>
    /// Forwards the item visitor to the wrapped component to resolve its base type for DTO serialization.
    /// </summary>
    public T Accept<T>(IItemVisitor<T> visitor) => _innerWeapon.Accept(visitor);

    /// <summary>
    /// Evaluates the capacity requirements of the underlying weapon and mounts it to the appropriate slot.
    /// </summary>
    public void Equip(Player player, IEquipSlot slot)
    {
        if (IsTwoHanded) 
            new TwoHandedSlot().Equip(this, player);
        else 
            slot.Equip(this, player);
    }

    /// <summary>
    /// Routes the combat visitor through the decorator chain while preserving the outermost object's identity.
    /// </summary>
    /// <remarks>
    /// Solves the standard Decorator/Visitor mismatch. It passes a proxy to the inner component 
    /// so that the inner component resolves the execution path (e.g., Heavy vs Light), 
    /// but the actual mathematical calculations use the modified stats of this outermost decorator.
    /// </remarks>
    public virtual void Accept(IAttackVisitor visitor)
    {
        var proxy = new DecoratorVisitorProxy(visitor, this);
        _innerWeapon.Accept(proxy);
    }

    /// <summary>
    /// An internal proxy enforcing the Object Identity rule during double dispatch.
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
/// A concrete decorator that applies a flat positive modifier to the base damage output.
/// </summary>
public class StrongModifier : WeaponDecorator
{
    public StrongModifier(IWeapon inner) : base(inner) { }
    public override string Name => _innerWeapon.Name + " (Strong)";
    public override int TotalDamage => _innerWeapon.TotalDamage + 5;
    public override int Damage => _innerWeapon.Damage + 5;
}

/// <summary>
/// A concrete decorator that applies a flat negative penalty to the luck stat.
/// </summary>
public class UnluckyModifier : WeaponDecorator
{
    public UnluckyModifier(IWeapon inner) : base(inner) { }
    public override string Name => _innerWeapon.Name + " (Unlucky)";
    public override int TotalLuck => _innerWeapon.TotalLuck - 5;
}