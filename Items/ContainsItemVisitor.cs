namespace AlchemyRPG;

public class ContainsItemVisitor : IItemVisitor<bool>
{
    private readonly Guid _targetId;
    public ContainsItemVisitor(Guid targetId) => _targetId = targetId;

    public bool VisitHeavyWeapon(IHeavyWeapon weapon) => false;
    public bool VisitLightWeapon(ILightWeapon weapon) => false;
    public bool VisitMagicWeapon(IMagicWeapon weapon) => false;
    public bool VisitSlottedWeapon(ISlottedWeapon weapon) => CheckSlots(weapon);
    public bool VisitHolder(ItemHolder holder) => CheckSlots(holder);
    public bool VisitGold(Gold gold) => false;
    public bool VisitCoin(Coin coin) => false;
    public bool VisitJunk(Junk junk) => false;
    public bool VisitUnknown(IItem item) => false;
    public bool VisitPassive(PassiveItem passive) => false;

    private bool CheckSlots(ISlotContainer container)
    {
        if (container.Id == _targetId) return true;
        return container.GetSlottedItems().Any(i => i.Id == _targetId || i.Accept(this));
    }
}