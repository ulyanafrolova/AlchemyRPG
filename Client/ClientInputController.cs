using System;

namespace AlchemyRPG;

/// <summary>
/// Defines a unified interface for all client input states.
/// Implements the State design pattern.
/// </summary>
public interface IInputState
{
    string GetPrompt();
    /// <summary>
    /// Processes the key press and returns an intent DTO if the input sequence is complete.
    /// Returns the next state of the system via the out parameter.
    /// </summary>
    /// <param name="key">The key pressed by the user.</param>
    /// <param name="player">The current state of the player.</param>
    /// <param name="nextState">The next state the context should transition to.</param>
    /// <returns>A valid NetworkCommandDTO if an action is ready to be sent; otherwise, null.</returns>
    NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState);
}

/// <summary>
/// Represents the context in the State design pattern.
/// Stores the current input state and manages state transitions.
/// </summary>
public class ClientInputController
{
    private IInputState _currentState;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientInputController"/> class.
    /// Sets the initial state to <see cref="NormalState"/>.
    /// </summary>
    public ClientInputController()
    {
        _currentState = new NormalState();
    }
    public string GetPrompt() => _currentState.GetPrompt();

    /// <summary>
    /// Processes a keystroke by delegating the logic to the current state,
    /// and updates the internal state pointer for the next tick.
    /// </summary>
    /// <param name="key">The key pressed by the user.</param>
    /// <param name="player">The current data transfer object representing the player.</param>
    /// <returns>A network command DTO if an action was finalized; otherwise, null.</returns>
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player)
    {
        var commandDto = _currentState.ProcessInput(key, player, out IInputState nextState);
        _currentState = nextState;
        return commandDto;
    }
}

/// <summary>
/// Represents the default input state where the player can move, pick up items, 
/// or initiate multi-step actions like attacking or equipping.
/// </summary>
public class NormalState : IInputState
{
    public string GetPrompt() => "";
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = this;

        if (key == Keybinds.Attack)
        {
            nextState = new WaitingForAttackDirectionState();
            return null;
        }

        if (key >= Keybinds.EquipBaseKey && key < Keybinds.EquipBaseKey + Keybinds.EquipSlotsCount)
        {
            int inventoryIndex = key - Keybinds.EquipBaseKey;
            if (inventoryIndex >= player.Backpack.Count)
                return null;

            nextState = new WaitingForEquipHandState(player.Backpack[inventoryIndex].Id);
            return null;
        }

        if (key == Keybinds.Drop)
        {
            // Transfer control to a new state to wait for the item index argument
            nextState = new WaitingForDropItemState();
            return null;
        }

        if (key == Keybinds.PickUp)
            return new PickUpCommandDTO();

        var (dx, dy) = key switch
        {
            Keybinds.MoveUp => (0, -1),
            Keybinds.MoveDown => (0, 1),
            Keybinds.MoveLeft => (-1, 0),
            Keybinds.MoveRight => (1, 0),
            _ => (0, 0)
        };

        if (dx != 0 || dy != 0)
            return new MoveCommandDTO { Dx = dx, Dy = dy };

        return null;
    }
}

/// <summary>
/// Represents a state where the client is waiting for the player to select the specific type of attack.
/// </summary>
public class WaitingForAttackTypeState : IInputState
{
    private readonly int _targetX;
    private readonly int _targetY;
    public string GetPrompt() => "Choose attack: [1] Normal, [2] Stealth, [3] Magic.";

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitingForAttackTypeState"/> class.
    /// </summary>
    /// <param name="targetX">The X coordinate of the attack target.</param>
    /// <param name="targetY">The Y coordinate of the attack target.</param>
    public WaitingForAttackTypeState(int targetX, int targetY)
    {
        _targetX = targetX;
        _targetY = targetY;
    }

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();

        AttackType? attackType = key switch
        {
            ConsoleKey.D1 => AttackType.Normal,
            ConsoleKey.D2 => AttackType.Stealth,
            ConsoleKey.D3 => AttackType.Magic,
            _ => null
        };

        if (attackType == null) 
            return null;

        return new AttackCommandDTO
        {
            TargetX = _targetX,
            TargetY = _targetY,
            AttackType = attackType.Value
        };
    }
}

/// <summary>
/// Represents a state where the client is waiting for the player to select 
/// which hand (left or right) to equip the chosen item into.
/// </summary>
public class WaitingForEquipHandState : IInputState
{
    public string GetPrompt() => "Which hand? [Q] Left / [R] Right";
    private readonly Guid _itemId;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitingForEquipHandState"/> class.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to be equipped.</param>
    public WaitingForEquipHandState(Guid itemId)
    {
        _itemId = itemId;
    }

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();
        
        int handSide = key switch
        {
            Keybinds.EquipLeft => 0,  // HandSide.Left
            Keybinds.EquipRight => 1, // HandSide.Right
            _ => -1
        };
        
        if (handSide == -1) 
            return null;

        return new EquipCommandDTO
        {
            ItemId = _itemId,
            HandSide = handSide
        };
    }
}

/// <summary>
/// Represents a state where the client is waiting for the player to choose the physical direction of their attack.
/// </summary>
public class WaitingForAttackDirectionState : IInputState
{
    public string GetPrompt() => "Attack direction? [W/A/S/D]";
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();

        var (dx, dy) = key switch
        {
            Keybinds.MoveUp => (0, -1),
            Keybinds.MoveDown => (0, 1),
            Keybinds.MoveLeft => (-1, 0),
            Keybinds.MoveRight => (1, 0),
            _ => (0, 0)
        };

        if (dx != 0 || dy != 0)
        {
            nextState = new WaitingForAttackTypeState(player.X + dx, player.Y + dy);
        }

        return null;
    }
}

/// <summary>
/// Represents a state where the client is waiting for the player to select an item to drop from their inventory.
/// </summary>
public class WaitingForDropItemState : IInputState
{
    public string GetPrompt() => "Drop which item? [0-9]";
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        // Regardless of the outcome (success or invalid input), 
        // always reset the state machine back to the default state.
        nextState = new NormalState();

        if (key >= Keybinds.EquipBaseKey && key < Keybinds.EquipBaseKey + Keybinds.EquipSlotsCount)
        {
            int inventoryIndex = key - Keybinds.EquipBaseKey;

            if (inventoryIndex >= player.Backpack.Count)
                return null;

            return new DropCommandDTO
            {
                ItemId = player.Backpack[inventoryIndex].Id
            };
        }

        return null;
    }
}