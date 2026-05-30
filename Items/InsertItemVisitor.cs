namespace AlchemyRPG;

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