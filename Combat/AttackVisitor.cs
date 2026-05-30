namespace AlchemyRPG;

/// <summary>
/// The base abstract class for all attack types in the Visitor pattern.
/// It holds the final calculated damage and defense values after a weapon is visited.
/// </summary>
public abstract class AttackVisitor : IAttackVisitor
{
    /// <summary>
    /// Gets or sets the final calculated damage the player will deal to the enemy.
    /// </summary>
    public int CalculatedDamage { get; protected set; }

    /// <summary>
    /// Gets or sets the final calculated defense the player has against the enemy's counter-attack.
    /// </summary>
    public int CalculatedDefense { get; protected set; }

    /// <summary>
    /// The player initiating the attack, whose RPG statistics are used in the calculations.
    /// </summary>
    protected Player _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackVisitor"/> class.
    /// </summary>
    /// <param name="player">The player performing the attack.</param>
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
/// Represents a standard physical attack. 
/// Scales well with strength for heavy weapons and dexterity for light weapons.
/// </summary>
public class NormalAttack : AttackVisitor
{
    public NormalAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.TotalDamage + _player.Strength + _player.Aggression;
        CalculatedDefense = _player.Strength + (_player.TotalLuck + weapon.TotalLuck);
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.TotalDamage + _player.Dexterity + (_player.TotalLuck + weapon.TotalLuck);
        CalculatedDefense = _player.Dexterity + (_player.TotalLuck + weapon.TotalLuck);
    }

    public override void VisitMagicWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.TotalDamage + _player.Wisdom;
        CalculatedDefense = _player.Dexterity + (_player.TotalLuck + weapon.TotalLuck);
    }

    public override void VisitNonWeapon()
    {
        CalculatedDamage = 0;
        CalculatedDefense = _player.Dexterity;
    }
}

/// <summary>
/// Represents a sneak attack.
/// Deals massive damage with light weapons but significantly reduces the effectiveness of heavy weapons.
/// </summary>
public class StealthAttack : AttackVisitor
{
    public StealthAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = (weapon.TotalDamage + _player.Strength + _player.Aggression) / 2;
        CalculatedDefense = _player.Strength;
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = (weapon.TotalDamage + _player.Dexterity + (_player.TotalLuck + weapon.TotalLuck)) * 2;
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
/// Represents a magical spell cast or strike.
/// Extremely effective with magic weapons, but renders physical weapons almost useless.
/// </summary>
public class MagicAttack : AttackVisitor
{
    public MagicAttack(Player player) : base(player) { }

    public override void VisitHeavyWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1;
        CalculatedDefense = (_player.TotalLuck + weapon.TotalLuck);
    }

    public override void VisitLightWeapon(IWeapon weapon)
    {
        CalculatedDamage = 1;
        CalculatedDefense = (_player.TotalLuck + weapon.TotalLuck);
    }

    public override void VisitMagicWeapon(IWeapon weapon)
    {
        CalculatedDamage = weapon.TotalDamage + _player.Wisdom;
        CalculatedDefense = _player.Wisdom * 2;
    }

    public override void VisitNonWeapon()
    {
        CalculatedDamage = 0;
        CalculatedDefense = _player.TotalLuck;
    }
}