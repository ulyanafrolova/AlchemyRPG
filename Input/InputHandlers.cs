namespace AlchemyRPG;

/// <summary>
/// Acts as the "Invoker" in the Command Design Pattern.
/// Maps physical keyboard keys to executable ICommand objects.
/// </summary>
public class InputHandler
{
    /// <summary>
    /// A registry storing the relationship between keyboard keys and their corresponding commands.
    /// </summary>
    private readonly Dictionary<ConsoleKey, ICommand> _commands = new();

    /// <summary>
    /// Initializes a new instance of the InputHandler.
    /// Pre-configures all available keybindings for the game environment.
    /// </summary>
    public InputHandler()
    {
        // Register Movement Commands 
        // Maps the standard WASD keys to directional movement commands (dx, dy)
        _commands[Keybinds.MoveUp] = new MoveCommand(0, -1);
        _commands[Keybinds.MoveDown] = new MoveCommand(0, 1);
        _commands[Keybinds.MoveLeft] = new MoveCommand(-1, 0);
        _commands[Keybinds.MoveRight] = new MoveCommand(1, 0);


        // Register Interaction Commands
        // Maps the interaction keys for picking up from the floor and dropping from inventory
        _commands[Keybinds.PickUp] = new PickUpCommand();
        _commands[Keybinds.Drop] = new DropCommand();

        // Register Help Command
        _commands[Keybinds.Help] = new HelpCommand();

        // Register Equip Commands 
        // This dynamically generates 10 unique EquipCommands
        for (int i = 0; i < Keybinds.EquipSlotsCount; i++)
        {
            _commands[Keybinds.EquipBaseKey + i] = new EquipCommand(i);
        }

        _commands[Keybinds.Attack] = new AttackCommand();
    }

    /// <summary>
    /// Processes the player's keyboard input, looks up the associated command, and executes it.
    /// </summary>
    /// <param name="key">The physical key pressed by the user.</param>
    /// <param name="state">The current global state of the game, passed into the executed command.</param>
    /// <returns>
    /// <c>true</c> if the game loop should continue running; 
    /// <c>false</c> if the game should terminate (e.g., when the Escape key is pressed).
    /// </returns>
    public bool HandleInput(ConsoleKey key, GameState state)
    {
        // Step 1: Check for the global exit signal
        // If the user presses Escape, we immediately return false to break the while-loop in Game.cs
        if (key == Keybinds.Cancel)
            return false;

        // Step 2: Attempt to resolve the command from the registry
        if (_commands.TryGetValue(key, out var command))
        {
            // Validation
            if (command.CanExecute(state))
            {
                // Execution
                command.Execute(state);
                state.Log = ""; // Clear global prompt on success
            }
        }
        else
        {
            // If the player presses an unbound key (e.g., 'M', 'Spacebar'), provide visual feedback
            state.Player.LogMessage = $"Unknown command '{key}'. Press a valid key.";
        }

        // Step 4: Continue the game
        return true; 
    }
}