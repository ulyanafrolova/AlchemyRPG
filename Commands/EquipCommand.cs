namespace AlchemyRPG;

/// <summary>
/// Represents a domain command that equips a specified item from the player's backpack to a designated hand.
/// </summary>
public class EquipCommand : ICommand
{
    private readonly Guid _itemId;
    private readonly HandSide _handSide;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipCommand"/> class.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to be equipped.</param>
    /// <param name="handSide">The specific hand (Left or Right) to equip the item into.</param>
    public EquipCommand(Guid itemId, HandSide handSide)
    {
        _itemId = itemId;
        _handSide = handSide;
    }

    /// <summary>
    /// Validates whether the equip action is permitted.
    /// Ensures the player is alive and that the requested item actually exists within their backpack.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player attempting to equip the item.</param>
    /// <returns>True if the item is found in the backpack; otherwise, false.</returns>
    public bool CanExecute(GameState state, Player executor)
    {
        if (executor.IsDead) return false;
        
        return executor.Backpack.Any(i => i.Id == _itemId);
    }

    /// <summary>
    /// Executes the equip action. Retrieves the item from the backpack, delegates the physical equipping 
    /// logic to the player object, and logs the successful action to the system logs.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player executing the command.</param>
    public void Execute(GameState state, Player executor)
    {
        var item = executor.Backpack.FirstOrDefault(i => i.Id == _itemId);
        
        if (item != null)
        {
            executor.TryEquipFromBackpack(_itemId, _handSide);
            state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} equipped [{_handSide}]: {item.Name}"));
        }
    }
}