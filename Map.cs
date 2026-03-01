using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

public class Map
{
    private readonly int _width = 40;
    private readonly int _height = 20;
    private readonly char[,] _grid;
    private readonly List<(int X, int Y, IItem Item)> _items = [];

    public Map()
    {
        _grid = new char[_height, _width];
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _grid[y, x] = (y == 0 || y == _height - 1 || x == 0 || x == _width - 1) ? '█' : ' ';
        AddItem(2, 2, new Gold(50));
        AddItem(3, 2, new Coin(10));
        AddItem(5, 5, new Dagger());
        AddItem(6, 5, new TwoHandedAxe());
        AddItem(8, 8, new Sword());

        AddItem(10, 10, new Junk("Skull"));
        AddItem(12, 4, new Junk("Old Bone"));
        AddItem(15, 15, new Junk("Broken Glass"));
    }
    public void PlaceItemAt(int x, int y, IItem item)
    {
        _items.Add((x, y, item));
    }
    public void AddItem(int x, int y, IItem item) => _items.Add((x, y, item));
    public void RemoveItem(int x, int y, IItem item) => _items.Remove((x, y, item));

    public List<IItem> GetItemsAt(int x, int y) => [.. _items.Where(i => i.X == x && i.Y == y).Select(i => i.Item)];

    public bool IsWalkable(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height && _grid[y, x] != '█';

    public void Draw(GameState state)
    {
        var p = state.Player;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (x == p.X && y == p.Y) Console.Write('¶');
                else
                {
                    var itemsHere = GetItemsAt(x, y);
                    if (itemsHere.Count != 0) Console.Write(itemsHere.First().Symbol);
                    else Console.Write(_grid[y, x]);
                }
            }
            Console.Write("   ");
            if (y == 0) Console.Write($"--- STATISTICS ---");
            else if (y == 1) Console.Write($"HP: {p.Health} | Strength: {p.Strength} | Dexterity: {p.Dexterity}");
            else if (y == 2) Console.Write($"Wisdom: {p.Wisdom} | Aggression: {p.Aggression} | Luck: {p.Luck}");
            else if (y == 3) Console.Write($"Gold: {p.Gold} | Coins: {p.Coins}");
            else if (y == 5) Console.Write($"--- HANDS ---");
            else if (y == 6) Console.Write($"Left: {(p.LeftHand != null ? p.LeftHand.Name : "Empty")}");
            else if (y == 7) Console.Write($"Right: {(p.RightHand != null ? p.RightHand.Name : "Empty")}");
            else if (y == 9) Console.Write($"--- INVENTORY (press a number to equip) ---");
            else if (y >= 10 && y < 10 + p.Backpack.Count && y < 19)
            {
                int index = y - 10;
                Console.Write($"[{index}] {p.Backpack[index].Name}");
            }
            Console.Write(new string(' ', 20));
            Console.WriteLine();
        }

        var standingOn = GetItemsAt(p.X, p.Y);
        if (standingOn.Count != 0)
        {
            string message = $"Standing on: {string.Join(", ", standingOn.Select(static i => i.Name))} (Press 'E' to pick up)";
            Console.WriteLine($"\n{message.PadRight(200)}");
            p.LogMessage = "";
        }
        else
            Console.WriteLine($"\n{new string(' ', 200)}");

        string fullLog = $"Log: {state.Log} {p.LogMessage}";
        Console.WriteLine(fullLog.PadRight(120));
        p.LogMessage = "";
        Console.WriteLine(new string('-', 60));
        Console.WriteLine(" CONTROLS:");
        Console.WriteLine(" [W, A, S, D] Move     | [E] Pick Up Item");
        Console.WriteLine(" [0-9] Equip Item      | [X] Drop Mode (Throw item)");
        Console.WriteLine(" [ESC] Exit Game");
        Console.WriteLine(new string('-', 60));
    }
}