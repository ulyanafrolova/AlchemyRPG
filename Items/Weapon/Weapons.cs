namespace AlchemyRPG;


/// <summary>
/// Represents a lightweight, single-handed weapon with low damage output.
/// </summary>
public class Dagger : BaseLightWeapon
{
    public override string Name => "Dagger";
    public override int Damage => 5;
    public override bool IsTwoHanded => false;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitLightWeapon((IWeapon)context);
}

/// <summary>
/// Represents a sword weapon that can be used in combat and supports heavy weapon interactions.
/// </summary>
public class Sword : BaseHeavyWeapon
{
    public override string Name => "Sword";
    public override int Damage => 10;
    public override bool IsTwoHanded => false;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitHeavyWeapon((IWeapon)context);
}

/// <summary>
/// Represents a two-handed axe weapon that deals heavy damage and requires both hands
/// </summary>
public class TwoHandedAxe : BaseHeavyWeapon
{
    public override string Name => "Two-Handed Axe";
    public override int Damage => 25;
    public override bool IsTwoHanded => true;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitHeavyWeapon((IWeapon)context);
}

/// <summary>
/// Represents a magical staff weapon that can be used to perform magic-based attacks.
/// </summary>
public class MagicStaff : BaseMagicWeapon
{
    public override string Name => "Magic Staff";
    public override int Damage => 8;
    public override bool IsTwoHanded => true;
    public override void Accept(IAttackVisitor visitor, IInventoryItem context) => visitor.VisitMagicWeapon((IWeapon)context);
}