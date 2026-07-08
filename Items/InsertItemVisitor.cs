namespace AlchemyRPG;

/// <summary>
/// Provides an item visitor that attempts to insert a specified inventory item into compatible containers or slotted
/// weapons.
/// </summary>
/// <remarks>Use this visitor to add an item to an inventory structure by visiting items that may accept new
/// contents. The visitor returns a value indicating whether the insertion was successful. Only item types that support
/// accepting new items, such as slotted weapons or item holders, will process the insertion; other item types will not
/// accept the item and the operation will return false.</remarks>
public class InsertItemVisitor : IItemVisitor<bool>
{
    private readonly IInventoryItem _itemToInsert;
    public InsertItemVisitor(IInventoryItem itemToInsert) => _itemToInsert = itemToInsert;
    public bool VisitHeavyWeapon(IHeavyWeapon weapon) => false;
    public bool VisitLightWeapon(ILightWeapon weapon) => false;
    public bool VisitMagicWeapon(IMagicWeapon weapon) => false;
    public bool VisitSlottedWeapon(ISlottedWeapon weapon) => weapon.AcceptItem(_itemToInsert);
    public bool VisitHolder(ItemHolder holder) => holder.AcceptItem(_itemToInsert);
    public bool VisitGold(Gold gold) => false;
    public bool VisitCoin(Coin coin) => false;
    public bool VisitJunk(Junk junk) => false;
    public bool VisitPassive(PassiveItem passive) => false;
    public bool VisitUnknown(IItem item) => false;
}