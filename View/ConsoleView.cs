using System;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Responsible for rendering the game world, statistics, and logs to the Windows console.
/// Acts as the primary implementation of the <see cref="IView"/> interface.
/// </summary>
public class ConsoleView : IView
{
    public string TutorialText => _tutorialText;
    private string[]? _cachedGrid;
    private static readonly ClientSymbolVisitor _clientSymbolVisitor = new();
    private string _instructions = "";
    private string _tutorialText = "";
    public bool IsInitialized { get; private set; } = false;

    public void InitializeFromState(GameStateDTO stateDto)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        IsInitialized = true;
    }

    public void Initialize(InitialDataDTO initialData)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        _cachedGrid = initialData.Grid;

        _instructions = initialData.Instructions;
        _tutorialText = initialData.TutorialText;

        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(Config.WindowWidth, Config.WindowHeight);
                Console.SetBufferSize(Config.WindowWidth, Config.WindowHeight);
            }
            catch { }
        }
        IsInitialized = true;
    }
    public void Render(GameStateDTO state, string localPrompt = "")
    {
        if (state?.Map == null || state.LocalPlayer == null)
            return;
        Console.SetCursorPosition(0, 0);
        var map = state.Map;
        var p = state.LocalPlayer;

        var itemsByCoord = map.Items
            .GroupBy(i => (i.X, i.Y))
            .ToDictionary(g => g.Key, g => g.ToList());

        var playersByCoord = state.OtherPlayers
                .ToDictionary(
                    op => (op.X, op.Y),
                    op => op.Id >= 1 && op.Id <= 9 ? op.Id.ToString()[0] : 'P'
                );

        var enemiesByCoord = state.Map.Enemies
            .ToDictionary(
            e => (e.X, e.Y),
            e => Tiles.Enemy);


        for (int y = 0; y < state.Map.Height; y++)
        {
            string mapLine = "";
            for (int x = 0; x < state.Map.Width; x++)
            {
                mapLine += GetSymbolToDraw(state, x, y, itemsByCoord, playersByCoord, enemiesByCoord);
            }
            string statsLine = GetStatsLine(y, p);
            Console.WriteLine($"{mapLine}   {statsLine}".PadRight(Config.PaddingRight));
        }

        Console.WriteLine("".PadRight(Config.PaddingRight));

        var standingOn = map.Items.Where(i => i.X == p.X && i.Y == p.Y).ToList();
        string groundInfo = standingOn.Count switch
        {
            > 1 => $"Ground: {standingOn.Count} items ({Tiles.SeveralItems}). Top: {standingOn.First().Name} (Press [{Keybinds.PickUp}])",
            1 => $"Ground: {standingOn.First().Name} (Press [{Keybinds.PickUp}])",
            _ => "Ground: Empty"
        };

        Console.WriteLine(groundInfo.PadRight(Config.PaddingRight));
        string activeLog = !string.IsNullOrEmpty(localPrompt) ? localPrompt : p.LogMessage;
        Console.WriteLine($"Log: {activeLog}".PadRight(Config.PaddingRight));
        Console.WriteLine(_instructions.PadRight(Config.PaddingRight));

        Console.WriteLine("--- RECENT EVENTS ---".PadRight(Config.PaddingRight));

        if (state.RecentEvents.Count == 0) Console.WriteLine("".PadRight(Config.PaddingRight));
        foreach (var log in state.RecentEvents)
        {
            Console.WriteLine(log.PadRight(Config.PaddingRight));
        }
    }
    public void RenderJournal(IReadOnlyList<string> entries)
    {
        Console.Clear();
        Console.WriteLine("=== ADVENTURE JOURNAL ===\n");
        if (entries.Count == 0)
        {
            Console.WriteLine("(No entries yet.)");
        }
        else
        {
            foreach (var entry in entries)
                Console.WriteLine(entry);
        }
        Console.WriteLine("\n--- Press any key to return ---");
        Console.ReadKey(true);
    }
    public void RenderFullScreen(string title, string content)
    {
        Console.Clear();
        Console.WriteLine($"{title}\n");
        Console.WriteLine(content);
        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("Press any key to start/return to the game...");
        Console.ReadKey(true);
    }

    private char GetSymbolToDraw(
        GameStateDTO state, int x, int y,
        Dictionary<(int, int), List<ItemDTO>> itemsByCoord,
        Dictionary<(int, int), char> playersByCoord,
        Dictionary<(int, int), char> enemiesByCoord)
    {
        itemsByCoord.TryGetValue((x, y), out var items);
        var coord = (x, y);

        var renderLayers = new Func<char?>[]
        {
        () => (state.LocalPlayer.X == x && state.LocalPlayer.Y == y) ? Tiles.Player : null,
        () => playersByCoord.TryGetValue(coord, out var ps) ? ps : null,
        () => enemiesByCoord.TryGetValue(coord, out var es) ? es : null,
        () => items != null && items.Count > 1 ? Tiles.SeveralItems : null,
        () => items != null && items.Count == 1 ? items[0].Accept(_clientSymbolVisitor) : null,
        () => _cachedGrid != null ? _cachedGrid[y][x] : ' '
        };
        return renderLayers.Select(l => l()).FirstOrDefault(s => s != null) ?? ' ';
    }
    private string GetStatsLine(int y, PlayerDTO p)
    {
        return y switch
        {
            0 => "--- STATISTICS ---",
            1 => $"HP: {p.Health} | STR: {p.Strength} | DEX: {p.Dexterity}",
            2 => $"WIS: {p.Wisdom} | AGR: {p.Aggression} | LUCK: {p.TotalLuck}",
            3 => $"Gold: {p.Gold} | Coins: {p.Coins}",
            5 => "--- HANDS ---",
            6 => $"Left: {(p.LeftHandName ?? "Empty")}",
            7 => $"Right: {(p.RightHandName ?? "Empty")}",
            9 => "--- INVENTORY ---",
            _ => (y >= 10 && y < 10 + p.BackpackItemNames.Count) ? $"[{y - 10}] {p.BackpackItemNames[y - 10]}" : ""
        };
    }
    public void Initialize()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;

        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(Config.WindowWidth, Config.WindowHeight);
                Console.SetBufferSize(Config.WindowWidth, Config.WindowHeight);
            }
            catch { }
        }
    }

    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }
}