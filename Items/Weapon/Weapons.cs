namespace AlchemyRPG;

/// <summary>
/// Represents a basic lightweight, one-handed weapon. 
/// Excels in stealth and dexterity-based combat calculations.
/// </summary>
public class Dagger : BaseLightWeapon
{
    public override string Name => "Dagger";
    public override int Damage => 5;
    public override bool IsTwoHanded => false;
    
    public override void Accept(IAttackVisitor visitor) => visitor.VisitLightWeapon(this);
}

/// <summary>
/// Represents a standard heavy, one-handed weapon. 
/// Balances strength scaling with the ability to hold an off-hand item.
/// </summary>
public class Sword : BaseHeavyWeapon
{
    public override string Name => "Sword";
    public override int Damage => 10;
    public override bool IsTwoHanded => false;
    
    public override void Accept(IAttackVisitor visitor) => visitor.VisitHeavyWeapon(this);
}

/// <summary>
/// Represents a massive heavy weapon that requires both hands to wield.
/// Deals high base damage and scales aggressively with strength.
/// </summary>
public class TwoHandedAxe : BaseHeavyWeapon
{
    public override string Name => "Two-Handed Axe";
    public override int Damage => 25;
    public override bool IsTwoHanded => true;
    
    public override void Accept(IAttackVisitor visitor) => visitor.VisitHeavyWeapon(this);
}

/// <summary>
/// Represents a magic-channeling weapon that requires both hands.
/// Bypasses physical armor mechanics in favor of wisdom-based combat scaling.
/// </summary>
public class MagicStaff : BaseMagicWeapon
{
    public override string Name => "Magic Staff";
    public override int Damage => 8;
    public override bool IsTwoHanded => true;
    
    public override void Accept(IAttackVisitor visitor) => visitor.VisitMagicWeapon(this);
}