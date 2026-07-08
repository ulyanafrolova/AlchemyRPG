using System;

namespace AlchemyRPG;

/// <summary>
/// Defines a unified interface for all client input states.
/// Implements the State design pattern.
/// </summary>
public interface IInputState
{
    string GetPrompt();
    NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState);
}

/// <summary>
/// Represents the context in the State design pattern.
/// </summary>
public class ClientInputController
{
    private IInputState _currentState;

    public ClientInputController()
    {
        _currentState = new NormalState();
    }
    public string GetPrompt() => _currentState.GetPrompt();

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player)
    {
        var commandDto = _currentState.ProcessInput(key, player, out IInputState nextState);
        _currentState = nextState;
        return commandDto;
    }
}

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

        if (key == Keybinds.Insert)
        {
            nextState = new WaitingForInsertSourceState();
            return null;
        }

        if (key == Keybinds.Drop)
        {
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

public class WaitingForAttackTypeState : IInputState
{
    private readonly int _targetX;
    private readonly int _targetY;
    public string GetPrompt() => "Choose attack: [1] Normal, [2] Stealth, [3] Magic.";

    public WaitingForAttackTypeState(int targetX, int targetY)
    {
        _targetX = targetX;
        _targetY = targetY;
    }

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();
        return key switch
        {
            ConsoleKey.D1 => new NormalAttackCommandDTO { TargetX = _targetX, TargetY = _targetY },
            ConsoleKey.D2 => new StealthAttackCommandDTO { TargetX = _targetX, TargetY = _targetY },
            ConsoleKey.D3 => new MagicAttackCommandDTO { TargetX = _targetX, TargetY = _targetY },
            _ => null
        };
    }
}

public class WaitingForEquipHandState : IInputState
{
    public string GetPrompt() => "Which hand? [Q] Left / [R] Right";
    private readonly Guid _itemId;

    public WaitingForEquipHandState(Guid itemId)
    {
        _itemId = itemId;
    }

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();

        int handSide = key switch
        {
            Keybinds.EquipLeft => 0,
            Keybinds.EquipRight => 1,
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

public class WaitingForDropItemState : IInputState
{
    public string GetPrompt() => "Drop which item? [0-9]";
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
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

public class WaitingForInsertSourceState : IInputState
{
    public string GetPrompt() => "Insert WHICH item? [0-9]";
    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();
        if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
        {
            int index = key - ConsoleKey.D0;
            if (index < player.Backpack.Count)
            {
                nextState = new WaitingForInsertTargetState(player.Backpack[index].Id);
            }
        }
        return null;
    }
}

public class WaitingForInsertTargetState : IInputState
{
    private readonly Guid _sourceId;
    public WaitingForInsertTargetState(Guid sourceId) => _sourceId = sourceId;
    public string GetPrompt() => "Insert INTO which container? [0-9]";

    public NetworkCommandDTO? ProcessInput(ConsoleKey key, PlayerDTO player, out IInputState nextState)
    {
        nextState = new NormalState();
        if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
        {
            int index = key - ConsoleKey.D0;
            if (index < player.Backpack.Count)
            {
                return new InsertCommandDTO
                {
                    ItemIdToInsert = _sourceId,
                    TargetContainerId = player.Backpack[index].Id
                };
            }
        }
        return null;
    }
}