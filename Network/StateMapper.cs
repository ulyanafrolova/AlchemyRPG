namespace AlchemyRPG;

/// <summary>
/// A lightweight snapshot of the game state used for network serialization.
/// Contains everything necessary to render a single frame on the client.
/// </summary>
public class SharedStateSnapshot
{
    public required MapDTO Map { get; init; }
    public required Dictionary<int, PlayerDTO> AllPlayers { get; init; }
    public required List<string> RecentEvents { get; init; }
    public required List<string> FullJournal { get; init; }
}

/// <summary>
/// Service responsible for transforming domain objects (Model) into Data Transfer Objects (DTOs)
/// and filtering them for specific clients.
/// </summary>
public class StateMapper
{
    private List<ItemDTO>? _cachedItemsDto = null;
    private int _lastItemsRevision = -1;
    private readonly ItemToDTOVisitor _dtoVisitor = new();

    /// <summary>
    /// Creates a complete snapshot of the game world. Caches item data based on revision 
    /// to optimize serialization performance.
    /// </summary>
    public SharedStateSnapshot CreateSnapshot(GameState state, IReadOnlyDictionary<int, Player> sessions, ILogger logger)
    {
        var currentItems = state.Map.GetAllItems();
        int currentRevision = state.Map.Version;

        if (_cachedItemsDto == null || currentRevision != _lastItemsRevision)
        {
            _cachedItemsDto = currentItems.Select(i =>
            {
                var dto = i.Item.Accept(_dtoVisitor);
                dto.X = i.X;
                dto.Y = i.Y;
                dto.Name = i.Item.Name;
                return dto;
            }).ToList();

            _lastItemsRevision = currentRevision;
        }

        var mapDto = new MapDTO
        {
            Width = state.Map.Width,
            Height = state.Map.Height,
            Items = _cachedItemsDto,
            Enemies = state.Map.Enemies.Select(e => new EntityDTO
            {
                X = e.X,
                Y = e.Y,
                Name = e.Name,
                Health = e.Health
            }).ToList()
        };

        var allPlayers = new Dictionary<int, PlayerDTO>(sessions.Count);
        foreach (var kvp in sessions)
        {
            var p = kvp.Value;
            allPlayers[kvp.Key] = new PlayerDTO
            {
                Id = kvp.Key,
                X = p.X,
                Y = p.Y,
                Name = p.Name,
                Health = p.Health,
                Strength = p.Strength,
                Dexterity = p.Dexterity,
                Wisdom = p.Wisdom,
                Aggression = p.Aggression,
                TotalLuck = p.TotalLuck,
                Gold = p.Gold,
                Coins = p.Coins,
                LeftHandName = p.LeftHand?.Name ?? "Empty",
                RightHandName = p.RightHand?.Name ?? "Empty",
                Backpack = p.Backpack.Select(i => new InventorySlotDTO { Id = i.Id, Name = i.Name }).ToList(),
                BackpackItemNames = p.Backpack.Select(i => i.Name).ToList(),
                LogMessage = p.LogMessage
            };
        }

        return new SharedStateSnapshot
        {
            Map = mapDto,
            AllPlayers = allPlayers,
            RecentEvents = state.EventLog.GetRecent().ToList(),
            FullJournal = logger.GetFullMemoryBuffer().Select(e => e.ToString()).ToList()
        };
    }

    /// <summary>
    /// Extracts static map layout and tutorial information for new clients during the initial handshake.
    /// </summary>
    public InitialDataDTO ExtractInitialData(GameState state)
    {
        string[] gridDto = new string[state.Map.Height];
        for (int y = 0; y < state.Map.Height; y++)
        {
            char[] row = new char[state.Map.Width];
            for (int x = 0; x < state.Map.Width; x++)
                row[x] = state.Map.GetTileAt(x, y).IsWalkable ? Tiles.Floor : Tiles.Wall; gridDto[y] = new string(row);
        }

        return new InitialDataDTO
        {
            Grid = gridDto,
            Instructions = state.ControlsText,
            TutorialText = state.TutorialText,
        };
    }

    /// <summary>
    /// Filters the snapshot so that the target player is labeled as 'LocalPlayer', 
    /// enabling the client to differentiate themselves from other entities.
    /// </summary>
    public GameStateDTO BuildForClient(SharedStateSnapshot snapshot, int targetPlayerId)
    {
        if (!snapshot.AllPlayers.TryGetValue(targetPlayerId, out var localPlayer))
            throw new InvalidOperationException($"[Mapper] Player {targetPlayerId} not found in snapshot.");

        var otherPlayers = new List<PlayerDTO>(snapshot.AllPlayers.Count - 1);
        foreach (var kvp in snapshot.AllPlayers)
        {
            if (kvp.Key == targetPlayerId) continue;
            otherPlayers.Add(kvp.Value);
        }

        return new GameStateDTO
        {
            Map = snapshot.Map,
            LocalPlayer = localPlayer,
            OtherPlayers = otherPlayers,
            RecentEvents = snapshot.RecentEvents,
            FullJournal = snapshot.FullJournal
        };
    }
}