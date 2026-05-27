using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents a human-controlled player character in the game world.
/// Manages RPG statistics, inventory, equipment, and network-related interaction logs.
/// </summary>
public class Player : Entity, IObserver<NoiseData>
{
    private ISubject<PlayerHeardNoiseData>? _playerHeardNoiseEvents;

    /// <summary>Gets the player's base physical strength.</summary>
    public int Strength { get; private set; } = 10;
    
    /// <summary>Gets the player's base dexterity and speed.</summary>
    public int Dexterity { get; private set; } = 10;
    
    /// <summary>Gets the player's base luck.</summary>
    public int Luck { get; private set; } = 5;

    /// <summary>
    /// Gets the player's total calculated luck, including innate stats and bonuses from equipped items.
    /// </summary>
    public int TotalLuck
    {
        get
        {
            int bonus = 0;
            if (RightHand != null) bonus += RightHand.LuckBonus;
            if (LeftHand != null) bonus += LeftHand.LuckBonus;
            return Luck + bonus;
        }
    }

    /// <summary>Gets the player's base aggression modifier for heavy attacks.</summary>
    public int Aggression { get; private set; } = 5;
    
    /// <summary>Gets the player's base wisdom for magical calculations.</summary>
    public int Wisdom { get; private set; } = 10;
    
    /// <summary>Gets the amount of standard coin currency the player holds.</summary>
    public int Coins { get; private set; } = 0;
    
    /// <summary>Gets the amount of premium gold currency the player holds.</summary>
    public int Gold { get; private set; } = 0;

    /// <summary>Adds the specified amount of coins to the player's purse.</summary>
    public void AddCoins(int amount) => Coins += amount;
    
    /// <summary>Adds the specified amount of gold to the player's purse.</summary>
    public void AddGold(int amount) => Gold += amount;

    private readonly List<IInventoryItem> _backpack = new();
    
    /// <summary>Gets a read-only view of the player's inventory items.</summary>
    public IReadOnlyList<IInventoryItem> Backpack => _backpack.AsReadOnly();

    /// <summary>Gets the item currently equipped in the player's left hand, if any.</summary>
    public IInventoryItem? LeftHand { get; private set; }
    
    /// <summary>Gets the item currently equipped in the player's right hand, if any.</summary>
    public IInventoryItem? RightHand { get; private set; }
    
    /// <summary>Gets the current local UI feedback message for this specific player.</summary>
    public string LogMessage { get; private set; } = "";

    /// <summary>
    /// Sets a temporary visual feedback message for the player (e.g., error prompts or interaction results).
    /// </summary>
    public void SetLogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        LogMessage = message;
    }

    /// <summary>
    /// Clears the current visual feedback message. Usually called at the beginning of a server tick.
    /// </summary>
    public void ClearLogMessage()
    {
        LogMessage = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="name">The player's display name.</param>
    /// <param name="startX">The initial horizontal starting coordinate.</param>
    /// <param name="startY">The initial vertical starting coordinate.</param>
    public Player(string name, int startX, int startY)
        : base(name, 100)
    {
        X = startX;
        Y = startY;
    }

    /// <summary>
    /// Updates the player's coordinates by applying a delta offset.
    /// </summary>
    public void Move(int dx, int dy) { X += dx; Y += dy; }

    /// <summary>
    /// Directly inserts an item into the player's backpack.
    /// </summary>
    public void AddToBackpack(IInventoryItem item) => _backpack.Add(item);

    /// <summary>
    /// Attempts to equip a specific item from the backpack to the specified hand.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="side">The hand to equip the item into.</param>
    public void TryEquipFromBackpack(Guid itemId, HandSide side)
    {
        var item = _backpack.FirstOrDefault(i => i.Id == itemId);
        if (item != null) item.Equip(this, side);
    }

    /// <summary>
    /// Removes a specified item from the backpack and unequips it if currently held.
    /// </summary>
    /// <returns>The removed item, or null if the item was not found.</returns>
    public IInventoryItem? DropItem(Guid itemId)
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

    /// <summary>
    /// Equips the item exclusively to the left hand, safely clearing two-handed holds.
    /// </summary>
    public void EquipLeftHand(IInventoryItem item)
    {
        if (RightHand != null && RightHand.IsTwoHanded) RightHand = null;
        LeftHand = item;
        if (RightHand == item) RightHand = null;
    }

    /// <summary>
    /// Equips the item exclusively to the right hand, safely clearing two-handed holds.
    /// </summary>
    public void EquipRightHand(IInventoryItem item)
    {
        if (LeftHand != null && LeftHand.IsTwoHanded) LeftHand = null;
        RightHand = item;
        if (LeftHand == item) LeftHand = null;
    }

    /// <summary>
    /// Equips a two-handed weapon, occupying both hand slots simultaneously.
    /// </summary>
    public void EquipTwoHanded(IInventoryItem item)
    {
        LeftHand = item;
        RightHand = item;
    }

    /// <summary>
    /// Wires the player into the acoustic event system so they can hear noises.
    /// </summary>
    public void InitializeHearing(ISubject<NoiseData> noiseEvents, ISubject<PlayerHeardNoiseData> heardNoiseEvents)
    {
        _playerHeardNoiseEvents = heardNoiseEvents;
        noiseEvents.Subscribe(this);
    }

    /// <summary>
    /// Handles incoming noise events and notifies the player-specific hearing bus if the noise reaches them.
    /// </summary>
    public void OnNext(NoiseData noise)
    {
        // Players ignore noises originating precisely from their own tile
        if (this.X == noise.SourceX && this.Y == noise.SourceY) return;

        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distance))
        {
            _playerHeardNoiseEvents?.Notify(
           new PlayerHeardNoiseData(this.Name, this.X, this.Y,
               noise.SourceX, noise.SourceY, distance));
        }
    }

    /// <summary>
    /// Safely detaches the player from the acoustic event system upon disconnection.
    /// </summary>
    public void TeardownHearing(ISubject<NoiseData> noiseEvents)
    {
        noiseEvents.Unsubscribe(this);
    }
}