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

    /// <summary>Non-weapon items that are not explicitly categorized yield a noise range of 0. </summary>
    public int VisitPassive(PassiveItem passive) => 0;

    /// <summary> Item holders are considered non-noisy since they are typically containers or accessories that do not produce significant sound when interacted with. </summary>
    public int VisitHolder(ItemHolder holder) => 0;

    /// <summary> Slotted weapons are treated as heavy, clunky items due to their modular nature and potential for attachments, resulting in a noise range of 5 when dropped or interacted with.</summary>
    public int VisitSlottedWeapon(ISlottedWeapon weapon) => 5;
}