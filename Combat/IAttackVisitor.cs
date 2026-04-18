namespace AlchemyRPG;

/// <summary>
/// This interface represents the "Visitor" in our combat system.
/// Instead of writing a huge 'switch' statement inside the Weapon class to check the attack type,
/// we pass this AttackVisitor to the weapon. The weapon then calls the correct method 
/// depending on its own type (Heavy, Light, Magic).
/// </summary>
public interface IAttackVisitor
{
    /// <summary>
    /// Calculates combat stats when the player uses a heavy weapon (like an Axe).
    /// </summary>
    void VisitHeavyWeapon(IWeapon weapon);
    /// <summary>
    /// Calculates combat stats when the player uses a light weapon (like a Dagger).
    /// </summary>
    void VisitLightWeapon(IWeapon weapon);
    /// <summary>
    /// Calculates combat stats when the player uses a magical weapon (like a Staff).
    /// </summary>
    void VisitMagicWeapon(IWeapon weapon);
    /// <summary>
    /// Calculates combat stats when the player is unarmed or holding a junk item.
    /// </summary>
    void VisitNonWeapon();
}
