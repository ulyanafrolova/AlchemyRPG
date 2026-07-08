using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents the authoritative domain entity for a player character.
/// </summary>
/// <remarks>
/// Acts as an Aggregate Root managing its own RPG statistics, inventory state, 
/// equipment conflict resolution, and spatial awareness (Observer of acoustic events).
/// </remarks>
public class Player : Entity, IObserver<NoiseData>
{
    private ISubject<PlayerHeardNoiseData>? _playerHeardNoiseEvents;

    private readonly int _baseStrength = 10;
    private readonly int _baseWisdom = 10;
    private readonly int _baseLuck = 5;

    // These properties dynamically aggregate base values with active equipment modifiers.
    // They rely on the structural safety of the equipment slots to avoid null reference issues.
    public int Strength => _baseStrength +
                           (LeftHand?.TotalStrength ?? 0) +
                           (RightHand?.TotalStrength ?? 0);

    public int Wisdom => _baseWisdom +
                         (LeftHand?.TotalWisdom ?? 0) +
                         (RightHand?.TotalWisdom ?? 0);

    public int TotalLuck => _baseLuck +
                            (LeftHand?.TotalLuck ?? 0) +
                            (RightHand?.TotalLuck ?? 0);

    public int Dexterity { get; private set; } = 10;
    public int Aggression { get; private set; } = 5;

    public int Coins { get; private set; } = 0;
    public int Gold { get; private set; } = 0;
    public void AddCoins(int amount) => Coins += amount;
    public void AddGold(int amount) => Gold += amount;
    private readonly List<IInventoryItem> _backpack = new();

    /// <summary>
    /// Exposes a read-only view of the inventory to prevent external mutation of the domain state.
    /// </summary>
    public IReadOnlyList<IInventoryItem> Backpack => _backpack.AsReadOnly();

    public IInventoryItem? LeftHand { get; private set; }
    public IInventoryItem? RightHand { get; private set; }

    /// <summary>
    /// Holds a transient localized feedback message generated during the current tick.
    /// Consumed and cleared by the UI rendering layer.
    /// </summary>
    public string LogMessage { get; private set; } = "";

    public void SetLogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        LogMessage = message;
    }

    public void ClearLogMessage()
    {
        LogMessage = string.Empty;
    }

    public Player(string name, int startX, int startY)
        : base(name, 100)
    {
        X = startX;
        Y = startY;
    }

    public void Move(int dx, int dy) { X += dx; Y += dy; }

    public void AddToBackpack(IInventoryItem item) => _backpack.Add(item);

    /// <summary>
    /// Attempts to equip an item from the inventory utilizing the Strategy pattern for slot resolution.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to equip.</param>
    /// <param name="slot">The specific equipment slot strategy to apply.</param>
    public void TryEquipFromBackpack(Guid itemId, IEquipSlot slot)
    {
        var item = _backpack.FirstOrDefault(i => i.Id == itemId);
        if (item != null) item.Equip(this, slot);
    }

    /// <summary>
    /// Safely detaches an item from the player's possession.
    /// </summary>
    /// <remarks>
    /// Ensures referential integrity by forcibly unequipping the item if it is currently held in either hand 
    /// before removing it from the inventory collection.
    /// </remarks>
    public IInventoryItem? RemoveFromInventory(Guid itemId)
    {
        var item = _backpack.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _backpack.Remove(item);
            if (LeftHand == item) LeftHand = null;
            if (RightHand == item) RightHand = null;
        }
        return item;
    }

    public IInventoryItem? DropItem(Guid itemId)
    {
        return RemoveFromInventory(itemId);
    }

    public void EquipLeftHand(IInventoryItem item)
    {
        // Enforce constraint: Unequip a two-handed weapon if equipping a one-handed item to this slot.
        if (RightHand != null && RightHand.IsTwoHanded) RightHand = null;
        LeftHand = item;
        // Prevent physical duplication: Cannot hold the exact same instance in both hands simultaneously.
        if (RightHand == item) RightHand = null;
    }

    public void EquipRightHand(IInventoryItem item)
    {
        if (LeftHand != null && LeftHand.IsTwoHanded) LeftHand = null;
        RightHand = item;
        if (LeftHand == item) LeftHand = null;
    }

    public void EquipTwoHanded(IInventoryItem item)
    {
        // Occupies both physical slots with the same object reference.
        LeftHand = item;
        RightHand = item;
    }

    private static readonly ItemRangeVisitor _rangeVisitor = new();

    /// <summary>
    /// Determines the maximum combat reach of the player based on equipped weapons.
    /// </summary>
    /// <remarks>
    /// Utilizes the <see cref="ItemRangeVisitor"/> to polymorphically extract the range 
    /// without resorting to type-checking abstractions.
    /// </remarks>
    public int AttackRange => Math.Max(
        LeftHand?.Accept(_rangeVisitor) ?? 1,
        RightHand?.Accept(_rangeVisitor) ?? 1
    );

    public void InitializeHearing(ISubject<NoiseData> noiseEvents, ISubject<PlayerHeardNoiseData> heardNoiseEvents)
    {
        _playerHeardNoiseEvents = heardNoiseEvents;
        noiseEvents.Subscribe(this);
    }

    /// <summary>
    /// Evaluates raw spatial noise data to determine if the player perceives it.
    /// </summary>
    public void OnNext(NoiseData noise)
    {
        // Ignore noise generated by the player's own actions to prevent redundant UI feedback.
        if (this.X == noise.SourceX && this.Y == noise.SourceY) return;

        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distance))
        {
            // Translate the raw domain noise into a player-specific feedback event for the client.
            _playerHeardNoiseEvents?.Notify(
                new PlayerHeardNoiseData(this.Name, this.X, this.Y, noise.SourceX, noise.SourceY, distance)
            );
        }
    }

    public void TeardownHearing(ISubject<NoiseData> noiseEvents)
    {
        noiseEvents.Unsubscribe(this);
    }

    /// <summary>
    /// Attempts to nest one inventory item inside another (e.g., placing a gem in a slotted sword).
    /// </summary>
    /// <returns>True if insertion constraints are met and the operation succeeds; otherwise, false.</returns>
    /// <remarks>
    /// Delegates constraint validation to <see cref="InsertItemVisitor"/> to ensure OCP compliance. 
    /// The source item is removed from the root inventory only if accepted by the container.
    /// </remarks>
    public bool TryInsertItem(Guid sourceItemId, Guid targetContainerId)
    {
        var itemToInsert = _backpack.FirstOrDefault(i => i.Id == sourceItemId);
        var targetContainer = _backpack.FirstOrDefault(i => i.Id == targetContainerId);

        if (itemToInsert == null || targetContainer == null) return false;

        var visitor = new InsertItemVisitor(itemToInsert);
        if (targetContainer.Accept(visitor))
        {
            _backpack.Remove(itemToInsert);
            return true;
        }
        return false;
    }
    public override void Accept(IEntityVisitor visitor) => visitor.VisitPlayer(this);
}