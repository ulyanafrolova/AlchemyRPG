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
        // Step 1: Update the global log to prompt the user for additional information
        state.Log = $"Drop which item? [{Keybinds.EquipKeysLabel}] or {Keybinds.Cancel} to cancel.";

        // Step 2: Force an immediate screen redraw so the player can read the prompt
        // We must reset the cursor to the top-left corner before drawing the frame
        Console.SetCursorPosition(0, 0);
        state.Map.Draw(state);

        // Step 3: Wait for the player to press a key
        var key = Console.ReadKey(true).Key;

        // Step 4: Process the user's input
        // Check if the pressed key is a number between 0 and 9
        if (key >= Keybinds.EquipBaseKey && key < Keybinds.EquipBaseKey + Keybinds.EquipSlotsCount)
        {
            // Convert the ConsoleKey enumeration (e.g., D0, D1) into an actual integer index (0, 1)
            int index = key - Keybinds.EquipBaseKey;

            // Attempt to remove the item from the backpack at the specified index
            var droppedItem = state.Player.DropItem(index);
            if (droppedItem != null)
            {
                state.Map.PlaceItemAt(state.Player.X, state.Player.Y, droppedItem);
                GameLogger.Instance.Log(LogType.Loot, $"{state.Player.Name} dropped {droppedItem.Name}");
            }
        }
        else
        {   
            GameLogger.Instance.Log(LogType.Loot, "Drop cancelled.");
        }
    }
}