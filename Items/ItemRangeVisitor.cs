namespace AlchemyRPG;

/// <summary>
/// Provides an item visitor that determines the range value for various item types.
/// </summary>
/// <remarks>This visitor returns the range of weapon items by accessing their Range property. For non-weapon
/// items, it returns a default range value of 1. This class is typically used to abstract the logic of retrieving range
/// values from different item implementations.</remarks>
public class ItemRangeVisitor : IItemVisitor<int>
{
    public int VisitHeavyWeapon(IHeavyWeapon weapon) => weapon.Range;
    public int VisitLightWeapon(ILightWeapon weapon) => weapon.Range;
    public int VisitMagicWeapon(IMagicWeapon weapon) => weapon.Range;
    public int VisitGold(Gold gold) => 1;
    public int VisitCoin(Coin coin) => 1;
    public int VisitJunk(Junk junk) => 1;
    public int VisitPassive(PassiveItem passive) => 1;
    public int VisitHolder(ItemHolder holder) => 1;
    public int VisitUnknown(IItem item) => 1;
    public int VisitSlottedWeapon(ISlottedWeapon weapon) => weapon.Range;
}