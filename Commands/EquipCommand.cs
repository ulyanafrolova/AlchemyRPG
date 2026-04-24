namespace AlchemyRPG;

/// <summary>
/// Represents an interactive command that equips a selected inventory item to a specific hand.
/// Temporarily pauses the main game loop to request additional user input (choosing left or right).
/// </summary>
public class EquipCommand : ICommand
{
    private readonly int _inventoryIndex;

    /// <summary>
    /// Initializes a new instance of the EquipCommand with the pre-selected inventory index.
    /// </summary>
    /// <param name="index">The index of the item inside the player's backpack to be equipped.</param>
    public EquipCommand(int index)
    {
        _inventoryIndex = index;
    }

    /// <summary>
    /// Validates the inventory index
    /// </summary>
    public bool CanExecute(GameState state)
    {
        if (_inventoryIndex >= state.Player.Backpack.Count)
        {
            state.Player.LogMessage = "Empty inventory slot.";
            return false;
        }
        return true;
    }

    /// <summary>
    /// Executes the equip action. Prompts the user to select a hand,
    /// and delegates the complex equipping logic to the player class.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    public void Execute(GameState state)
    {
        // Step 1: Prompt for additional information
        // We dynamically insert the item's name into the prompt 
        state.Log = $"Equipping {state.Player.Backpack[_inventoryIndex].Name}. Which hand? [{Keybinds.EquipLeft}] Left / [{Keybinds.EquipRight}] Right";

        // Step 2: Force an immediate screen redraw to display the prompt
        Console.SetCursorPosition(0, 0);
        state.Map.Draw(state);

        // Step 3: Wait for the player's decision
        var handKey = Console.ReadKey(true).Key;

        // Step 4: Process the input and execute the corresponding equip logic
        if (handKey == Keybinds.EquipLeft)
        {
            state.Player.TryEquipFromBackpack(_inventoryIndex, HandSide.Left);
        }
        else if (handKey == Keybinds.EquipRight)
        {
            state.Player.TryEquipFromBackpack(_inventoryIndex, HandSide.Right);
        }
        else
            GameLogger.Instance.Log(LogType.Loot, "Cancelled equipping.");
    }
}