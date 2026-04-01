namespace AlchemyRPG;

/// <summary>
/// The base class for all attack types. It holds the final calculated numbers for damage and defense.
/// When the player attacks, the weapon will call one of the Visit methods to fill these numbers.
/// </summary>
public abstract class AttackVisitor : IAttackVisitor
{
    // The final damage the player will do to the enemy
    public int CalculatedDamage { get; protected set; }
    // The final defense the player has against the enemy's attack
    public int CalculatedDefense { get; protected set; }
    // We need the player's stats to calculate the final numbers
    protected Player _player;

    public AttackVisitor(Player player)
    {
        _player = player;
    }
    public abstract void VisitHeavyWeapon(IWeapon weapon);
    public abstract void VisitLightWeapon(IWeapon weapon);
    public abstract void VisitMagicWeapon(IWeapon weapon);
    public abstract void VisitNonWeapon();
}

/// <summary>
/// Represents a standard physical attack
/// </summary>
public class NormalAttack : AttackVisitor
{
    public NormalAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.Damage + _player.Strength + _player.Aggression;
        CalculatedDefense = _player.Strength + (_player.Luck + weapon.LuckBonus);
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.Damage + _player.Dexterity + (_player.Luck + weapon.LuckBonus);
        CalculatedDefense = _player.Dexterity + (_player.Luck + weapon.LuckBonus);
    }

    public override void VisitMagicWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1; 
        CalculatedDefense = _player.Dexterity + (_player.Luck + weapon.LuckBonus);
    }

    public override void VisitNonWeapon()
    {
        CalculatedDamage = 0; 
        CalculatedDefense = _player.Dexterity;
    }
}

/// <summary>
/// Represents a sneak attack
/// </summary>
public class StealthAttack : AttackVisitor
{
    public StealthAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = (weapon.Damage + _player.Strength + _player.Aggression) / 2;
        CalculatedDefense = _player.Strength;
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = (weapon.Damage + _player.Dexterity + (_player.Luck + weapon.LuckBonus)) * 2;
        CalculatedDefense = _player.Dexterity;
    }

    public override void VisitMagicWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1;
        CalculatedDefense = 0;
    }

    public override void VisitNonWeapon()
    {
        CalculatedDamage = 0;
        CalculatedDefense = 0;
    }
}

/// <summary>
/// Represents a spell cast or magical strike
/// </summary>
public class MagicAttack : AttackVisitor
{
    public MagicAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1;
        CalculatedDefense = (_player.Luck + weapon.LuckBonus);
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1;
        CalculatedDefense = (_player.Luck + weapon.LuckBonus);
    }

    public override void VisitMagicWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.Damage + _player.Wisdom;
        CalculatedDefense = _player.Wisdom * 2;
    }

    public override void VisitNonWeapon()
    {
        CalculatedDamage = 0;
        CalculatedDefense = _player.Luck;
    }
}