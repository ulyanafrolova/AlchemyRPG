namespace AlchemyRPG;

/// <summary>
/// Extends the basic inventory item contract for weapons.
/// </summary>
public interface IWeapon : IInventoryItem
{
    int Damage { get; }
    int LuckBonus { get; } 
}
/// <summary>
/// Interface for heavy weapons
/// </summary>
public interface IHeavyWeapon : IWeapon { }
/// <summary>
/// Interface for light weapons
/// </summary>
public interface ILightWeapon : IWeapon { }
/// <summary>
/// Interface for magic weapons
/// </summary>
public interface IMagicWeapon : IWeapon { }