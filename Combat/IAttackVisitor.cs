namespace AlchemyRPG;

/// <summary>
/// Represents the Visitor interface in the combat system.
/// Allows the game to calculate damage and defense based on the specific type of weapon equipped,
/// without requiring type checking or hardcoded switch statements.
/// </summary>
public interface IAttackVisitor
{
    /// <summary>
    /// Performs attack calculations when the equipped item is a heavy weapon.
    /// </summary>
    void VisitHeavyWeapon(IWeapon weapon);

    /// <summary>
    /// Performs attack calculations when the equipped item is a light weapon.
    /// </summary>
    void VisitLightWeapon(IWeapon weapon);

    /// <summary>
    /// Performs attack calculations when the equipped item is a magic weapon.
    /// </summary>
    void VisitMagicWeapon(IWeapon weapon);

    /// <summary>
    /// Performs attack calculations when the player is unarmed or holding a non-weapon item.
    /// </summary>
    void VisitNonWeapon();
}