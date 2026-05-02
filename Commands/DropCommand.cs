namespace AlchemyRPG;

/// <summary>
/// Represents an interactive command that allows the player to drop an item from their backpack.
/// Temporarily pauses the main game loop to request additional user input (the inventory index).
/// </summary>
public class DropCommand : ICommand
{
    /// <summary>
    /// Validation
    /// </summary>
    public bool CanExecute(GameState state)
    {
        if (state.Player.Backpack.Count == 0)
        {
            state.Player.LogMessage = "Your backpack is empty. Nothing to drop.";
            return false;
        }
        return true;
    }
    /// <summary>
    /// Executes the drop action. Prompts the user for a numeric input and transfers the item from the backpack to the map grid.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    public void Execute(GameState state)
    {
        state.Log = $"Drop which item? [{Keybinds.EquipKeysLabel}] or {Keybinds.Cancel} to cancel.";

        state.IsWaitingForSecondaryInput = true;
        state.PendingAction = (key) => ProcessDropInput(state, key);
    }

    private void ProcessDropInput(GameState state, ConsoleKey key)
    {
        state.IsWaitingForSecondaryInput = false;
        state.PendingAction = null;
        state.Log = "";

        if (key >= Keybinds.EquipBaseKey && key < Keybinds.EquipBaseKey + Keybinds.EquipSlotsCount)
        {
            int index = key - Keybinds.EquipBaseKey;
            var droppedItem = state.Player.DropItem(index);
            if (droppedItem != null)
            {
                state.Map.PlaceItemAt(state.Player.X, state.Player.Y, droppedItem);
                GameLogger.Instance.Log(LogType.Loot, $"{state.Player.Name} dropped {droppedItem.Name}");
                state.Log = $"Dropped: {droppedItem.Name}";
                if (droppedItem.NoiseRange > 0)
                {
                    GameLogger.Instance.Log(LogType.Loot, $"Drop noise generated: {droppedItem.NoiseRange}");

                    var acousticMap = AcousticSystem.CalculateAcousticDistances(
                        state.Map, state.Player.X, state.Player.Y, droppedItem.NoiseRange);

                    var noiseData = new NoiseData(state.Player.X, state.Player.Y, acousticMap);
                    state.NoiseEvents.Notify(noiseData);
                }
            }
        }
        else
        {
            GameLogger.Instance.Log(LogType.Loot, "Drop cancelled.");
        }
    }
}