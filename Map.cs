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

        AddItem(10, 10, new Skull());
        AddItem(12, 4, new OldBone());
        AddItem(15, 15, new BrokenGlass());
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
            string mapLine = "";
            for (int x = 0; x < _width; x++)
            {
                if (x == p.X && y == p.Y)
                    mapLine += "¶";
                else
                {
                    var itemsHere = GetItemsAt(x, y);
                    if (itemsHere.Count != 0) mapLine += itemsHere.First().Symbol;
                    else mapLine += _grid[y, x];
                }
            }
            string statsLine = GetStatsLine(y, p);
            string fullLine = $"{mapLine}   {statsLine}";
            Console.WriteLine(fullLine.PadRight(100));
        }
        Console.WriteLine(new string('-', 80));
        var standingOn = GetItemsAt(p.X, p.Y);
        string groundInfo = standingOn.Count > 0
            ? $"Ground: {standingOn.First().Name} (Press E)"
            : "Ground: Empty";

        Console.WriteLine(groundInfo.PadRight(100));

        string logInfo = $"Log: {state.Log} {p.LogMessage}";
        Console.WriteLine(logInfo.PadRight(100));
        p.LogMessage = ""; 

        Console.WriteLine("Controls: [WASD] Move | [E] Pick Up | [X] Drop | [0-9] Equip".PadRight(100));
    }

    private static string GetStatsLine(int y, Player p)
    {
        return y switch
        {
            0 => "--- STATISTICS ---",
            1 => $"HP: {p.Health} | STR: {p.Strength} | DEX: {p.Dexterity}",
            2 => $"WIS: {p.Wisdom} | AGR: {p.Aggression} | LUCK: {p.Luck}",
            3 => $"Gold: {p.Gold} | Coins: {p.Coins}",
            5 => "--- HANDS ---",
            6 => $"Left: {(p.LeftHand?.Name ?? "Empty")}",
            7 => $"Right: {(p.RightHand?.Name ?? "Empty")}",
            9 => "--- INVENTORY ---",
            _ => (y >= 10 && y < 10 + p.Backpack.Count)
                 ? $"[{y - 10}] {p.Backpack[y - 10].Name}"
                 : ""
        };
    }
}