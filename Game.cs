using System;
using System.Linq;

namespace AlchemyRPG;

public class Game
{
    private readonly GameState _state;
    private bool _isRunning = true;
    private bool _isDropMode = false;

    public Game()
    {
        _state = new GameState
        {
            Player = new Player(1, 1),
            Map = new Map()
        };
    }

    public void Run()
    {
        Console.CursorVisible = false;
        Console.Clear();

        while (_isRunning)
        {
            Console.SetCursorPosition(0, 0);
            if (_isDropMode)
                _state.Log = "DROP MODE: Press a number (0-9) to drop item, or ESC to cancel.";

            _state.Map.Draw(_state);
            HandleInput();
        }
    }

    private void HandleInput()
    {
        var keyInfo = Console.ReadKey(true);
        var key = keyInfo.Key;
        int dx = 0, dy = 0;

        if (_isDropMode)
        {
            if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
            {
                int index = key - ConsoleKey.D0;

                var itemsOnFloor = _state.Map.GetItemsAt(_state.Player.X, _state.Player.Y);
                if (itemsOnFloor.Count != 0)
                {
                    _state.Log = "Cannot drop: Floor is occupied!";
                    _isDropMode = false;
                    return;
                }

                var droppedItem = _state.Player.DropItem(index);
                if (droppedItem != null)
                {
                    _state.Map.PlaceItemAt(_state.Player.X, _state.Player.Y, droppedItem);

                }
                _state.Log = "";
                _isDropMode = false;
            }
            else if (key == ConsoleKey.Escape || key == ConsoleKey.X)
            {
                _isDropMode = false;
                _state.Log = "";
            }
            return;
        }

        if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
        {
            int index = key - ConsoleKey.D0;

            _state.Log = "Equip to which hand? [Q] Left / [E] Right";
            Console.SetCursorPosition(0, 0);
            _state.Map.Draw(_state);

            var handKey = Console.ReadKey(true).Key;

            if (handKey == ConsoleKey.Q)
            {
                _state.Player.TryEquipFromBackpack(index, HandSide.Left);
                _state.Log = "";
            }
            else if (handKey == ConsoleKey.E)
            {
                _state.Player.TryEquipFromBackpack(index, HandSide.Right);
                _state.Log = "";
            }
            else
            {
                _state.Log = "Cancelled equipping.";
            }
            return;
        }

        switch (key)
        {
            case ConsoleKey.W: dy = -1; break;
            case ConsoleKey.S: dy = 1; break;
            case ConsoleKey.A: dx = -1; break;
            case ConsoleKey.D: dx = 1; break;

            case ConsoleKey.X:
                _isDropMode = true;
                break;

            case ConsoleKey.E:
                var items = _state.Map.GetItemsAt(_state.Player.X, _state.Player.Y);
                if (items.Count != 0) items.First().OnPickUp(_state);
                break;

            case ConsoleKey.Escape: _isRunning = false; break;
        }

        if (dx != 0 || dy != 0)
        {
            if (_state.Map.IsWalkable(_state.Player.X + dx, _state.Player.Y + dy))
                _state.Player.Move(dx, dy);
        }
    }
}