namespace AlchemyRPG;

/// <summary>
/// A concrete visitor implementation that determines the acoustic noise range generated 
/// by specific items when they are dropped or interacted with in the environment.
/// </summary>
public class ItemNoiseVisitor : IItemVisitor<int>
{
    /// <summary>Returns a large noise range (5) for heavy, clunky weapons.</summary>
    public int VisitHeavyWeapon(IHeavyWeapon weapon) => 5;
    
    /// <summary>Returns a minimal noise range (1) for lightweight, subtle weapons.</summary>
    public int VisitLightWeapon(ILightWeapon weapon) => 1;
    
    /// <summary>Returns a moderate noise range (3) for magical weapons.</summary>
    public int VisitMagicWeapon(IMagicWeapon weapon) => 3;
    
    /// <summary>Gold yields a noise range of 0 when dropped.</summary>
    public int VisitGold(Gold gold) => 0;
    
    /// <summary>Coins yield a noise range of 0 when dropped.</summary>
    public int VisitCoin(Coin coin) => 0;
    
    /// <summary>Junk items yield a noise range of 0 when dropped.</summary>
    public int VisitJunk(Junk junk) => 0;
    
    /// <summary>Unknown items default to a noise range of 0.</summary>
    public int VisitUnknown(IItem item) => 0;
}