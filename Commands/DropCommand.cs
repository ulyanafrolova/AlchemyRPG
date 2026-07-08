namespace AlchemyRPG;

/// <summary>
/// Represents a command to remove a specific item from the player's inventory and place it onto the game map.
/// Also handles the calculation and broadcasting of noise events resulting from the item hitting the ground.
/// </summary>
public class DropCommand : ICommand
{
    private readonly Guid _itemId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropCommand"/> class.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to be dropped.</param>
    public DropCommand(Guid itemId)
    {
        _itemId = itemId;
    }

    /// <summary>
    /// Validates whether the item can be dropped.
    /// Ensures the player is alive and actually possesses the item in their backpack.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player attempting to drop the item.</param>
    /// <returns>True if the item is present in the inventory; otherwise, false.</returns>
    public bool CanExecute(GameState state, Player executor)
    {
        if (executor.IsDead) return false;
        return executor.Backpack.Any(i => i.Id == _itemId);
    }

    /// <summary>
    /// Executes the drop action. Removes the item from the player, places it on their current coordinates,
    /// logs the event, and triggers the acoustic system if the item generates noise.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player dropping the item.</param>
    public void Execute(GameState state, Player executor)
    {
        var droppedItem = executor.DropItem(_itemId);

        if (droppedItem != null)
        {
            state.Map.PlaceItemAt(executor.X, executor.Y, droppedItem);

            state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} dropped: {droppedItem.Name}"));
            state.EventLog.Push($"{executor.Name} dropped: {droppedItem.Name}");

            // Calculate potential noise using the Visitor pattern
            int noiseRange = droppedItem.Accept(new ItemNoiseVisitor());

            if (noiseRange > 0)
            {
                state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"Noise generated: {noiseRange}"));
                state.EventLog.Push($"{executor.Name} made noise (Range: {noiseRange})");

                // Calculate the sound propagation map and notify observers
                var acousticMap = state.Acoustic.CalculateAcousticDistances(state.Map, executor.X, executor.Y, noiseRange);
                var noiseData = new NoiseData(executor.X, executor.Y, acousticMap);
                state.NoiseEvents.Notify(noiseData);
            }
        }
    }
}