namespace AlchemyRPG;

/// <summary>
/// Extends the basic inventory item contract specifically for weapons, introducing combat capabilities
/// </summary>
public interface IWeapon : IInventoryItem
{
    /// <summary>
    /// Gets the base damage output of the weapon
    /// </summary>
    int Damage { get; }
}

/// <summary>
/// A marker interface denoting a heavy, strength-based weapon
/// Used strictly for Visitor pattern type resolution
/// </summary>
public interface IHeavyWeapon : IWeapon { }

/// <summary>
/// A marker interface denoting a light, dexterity-based weapon
/// Used strictly for Visitor pattern type resolution
/// </summary>
public interface ILightWeapon : IWeapon { }

/// <summary>
/// A marker interface denoting a magic, wisdom-based weapon
/// Used strictly for Visitor pattern type resolution
/// </summary>
public interface IMagicWeapon : IWeapon { }