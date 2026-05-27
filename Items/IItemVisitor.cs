namespace AlchemyRPG;

/// <summary>
/// Defines a generic contract for the Visitor design pattern applied to interactable items.
/// Enables operations to be performed on items based on their concrete types 
/// without modifying their underlying class structures.
/// </summary>
/// <typeparam name="T">The return type of the visitor operations.</typeparam>
public interface IItemVisitor<T>
{
    /// <summary>Executes the operation for a heavy weapon.</summary>
    T VisitHeavyWeapon(IHeavyWeapon weapon);
    
    /// <summary>Executes the operation for a light weapon.</summary>
    T VisitLightWeapon(ILightWeapon weapon);
    
    /// <summary>Executes the operation for a magic weapon.</summary>
    T VisitMagicWeapon(IMagicWeapon weapon);
    
    /// <summary>Executes the operation for premium gold currency.</summary>
    T VisitGold(Gold gold);
    
    /// <summary>Executes the operation for standard coin currency.</summary>
    T VisitCoin(Coin coin);
    
    /// <summary>Executes the operation for useless junk items.</summary>
    T VisitJunk(Junk junk);
    
    /// <summary>Executes a fallback operation for an unknown or unclassified item.</summary>
    T VisitUnknown(IItem item);
}

/// <summary>
/// A concrete visitor implementation that translates domain-level item instances 
/// into their network-safe Data Transfer Object (DTO) equivalents.
/// Used extensively during game state serialization prior to network broadcasting.
/// </summary>
public class ItemToDTOVisitor : IItemVisitor<ItemDTO>
{
    public ItemDTO VisitHeavyWeapon(IHeavyWeapon weapon) => new WeaponDTO();
    public ItemDTO VisitLightWeapon(ILightWeapon weapon) => new WeaponDTO();
    public ItemDTO VisitMagicWeapon(IMagicWeapon weapon) => new WeaponDTO();
    public ItemDTO VisitGold(Gold gold) => new GoldDTO();
    public ItemDTO VisitCoin(Coin coin) => new CoinDTO();
    public ItemDTO VisitJunk(Junk junk) => new JunkDTO();
    public ItemDTO VisitUnknown(IItem item) => new JunkDTO(); 
}