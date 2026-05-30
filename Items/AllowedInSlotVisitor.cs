namespace AlchemyRPG;

/// <summary>
/// Evaluates whether a specific item type is permitted to be inserted into a slot container 
/// (such as an <see cref="ISlottedWeapon"/> or an <see cref="ItemHolder"/>).
/// </summary>
/// <remarks>
/// Acts as a concrete Visitor resolving type constraints via double dispatch. 
/// This guarantees that slot filtering is strictly enforced without relying on 
/// runtime type identification (RTTI) or type-casting branches (e.g., is/as/switch), 
/// maintaining pure polymorphism and preserving the Open-Closed Principle.
/// </remarks>
public class AllowedInSlotVisitor : IItemVisitor<bool>
{
    /// <summary>
    /// Allows stat-boosting modifiers to be slotted into the container.
    /// </summary>
    public bool VisitPassive(PassiveItem passive) => true;

    /// <summary>
    /// Allows nested containers to be slotted, enabling a deep Composite pattern structure.
    /// </summary>
    public bool VisitHolder(ItemHolder holder) => true;

    // Primary weapons, currencies, and non-interactable objects cannot act as slot modifiers.
    public bool VisitHeavyWeapon(IHeavyWeapon weapon) => false;
    public bool VisitLightWeapon(ILightWeapon weapon) => false;
    public bool VisitMagicWeapon(IMagicWeapon weapon) => false;
    public bool VisitGold(Gold gold) => false;
    public bool VisitCoin(Coin coin) => false;
    public bool VisitJunk(Junk junk) => false;

    /// <summary>
    /// Prevents logical paradoxes and infinite recursion by forbidding slotted weapons 
    /// from being placed inside other slotted containers.
    /// </summary>
    public bool VisitSlottedWeapon(ISlottedWeapon weapon) => false;

    /// <summary>
    /// Default fallback for unidentified implementations to ensure closed-by-default security.
    /// </summary>
    public bool VisitUnknown(IItem item) => false;
}