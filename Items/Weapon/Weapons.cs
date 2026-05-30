namespace AlchemyRPG;

/// <summary>
/// Represents a basic lightweight, one-handed weapon. 
/// Excels in stealth and dexterity-based combat calculations.
/// </summary>
public class Dagger : BaseLightWeapon
{
    public override string Name => "Dagger";
    public override int Damage => 5;
    public override bool IsTwoHanded => false;

    public override void Accept(IAttackVisitor visitor) => visitor.VisitLightWeapon(this);
}

/// <summary>
/// Represents a standard heavy, one-handed weapon. 
/// Balances strength scaling with the ability to hold an off-hand item.
/// </summary>
public class Sword : BaseHeavyWeapon
{
    public override string Name => "Sword";
    public override int Damage => 10;
    public override bool IsTwoHanded => false;

    public override void Accept(IAttackVisitor visitor) => visitor.VisitHeavyWeapon(this);
}

/// <summary>
/// Represents a massive heavy weapon that requires both hands to wield.
/// Deals high base damage and scales aggressively with strength.
/// </summary>
public class TwoHandedAxe : BaseHeavyWeapon
{
    public override string Name => "Two-Handed Axe";
    public override int Damage => 25;
    public override bool IsTwoHanded => true;

    public override void Accept(IAttackVisitor visitor) => visitor.VisitHeavyWeapon(this);
}

/// <summary>
/// Represents a magic-channeling weapon that requires both hands.
/// Bypasses physical armor mechanics in favor of wisdom-based combat scaling.
/// </summary>
public class MagicStaff : BaseMagicWeapon
{
    public override string Name => "Magic Staff";
    public override int Damage => 8;
    public override bool IsTwoHanded => true;

    public override void Accept(IAttackVisitor visitor) => visitor.VisitMagicWeapon(this);
}

/// <summary>
/// A heavy weapon acting as a Composite node, allowing runtime insertion of stat-modifying items.
/// </summary>
public class SlottedSword : BaseHeavyWeapon, ISlottedWeapon
{
    public override string Name => "Slotted Broadsword";
    public override int Damage => 12;
    public override bool IsTwoHanded => false;
    private readonly int _maxSlots = 2;
    /// <summary>
    /// The internal collection of nested items acting as the Composite tree branches
    /// </summary>
    private readonly List<IInventoryItem> _slots = new();
    // Aggregates statistics recursively from all child nodes
    public override int TotalDamage => Damage + _slots.Sum(i => i.TotalDamage);
    public override int TotalStrength => StrengthBonus + _slots.Sum(i => i.TotalStrength);
    public override int TotalWisdom => WisdomBonus + _slots.Sum(i => i.TotalWisdom);
    public override int TotalLuck => LuckBonus + _slots.Sum(i => i.TotalLuck);
    /// <summary>
    /// Attempts to nest a new item into this container.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <returns>True if the insertion complies with capacity, type, and structural integrity rules; otherwise, false.</returns>
    /// <remarks>
    /// Execution halts if capacity is reached. 
    /// Relies on <see cref="AllowedInSlotVisitor"/> to enforce domain type-safety without casting.
    /// Utilizes <see cref="ContainsItemVisitor"/> to detect and reject cyclical reference graphs (e.g., putting a container inside itself), preventing StackOverflow exceptions during aggregation.
    /// </remarks>
    public bool AcceptItem(IInventoryItem item)
    {
        if (_slots.Count >= _maxSlots) return false;
        var validator = new AllowedInSlotVisitor();
        if (!item.Accept(validator)) return false;

        var cycleChecker = new ContainsItemVisitor(this.Id);
        if (item.Id == this.Id || item.Accept(cycleChecker)) return false;

        _slots.Add(item);
        return true;
    }

    public IReadOnlyList<IInventoryItem> GetSlottedItems() => _slots.AsReadOnly();
    public override T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitSlottedWeapon(this);
    public override void Accept(IAttackVisitor visitor) => visitor.VisitHeavyWeapon(this);
}