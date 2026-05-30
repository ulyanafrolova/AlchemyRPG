namespace AlchemyRPG;

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