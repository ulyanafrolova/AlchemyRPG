using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AlchemyRPG;

/// <summary>
/// Manages the TCP server infrastructure, handling incoming client connections, 
/// routing network commands to the game engine, and broadcasting state snapshots.
/// </summary>
public class NetworkServer : IDisposable
{
    private readonly StateMapper _stateMapper = new();
    private readonly ICommandMapper _commandMapper = new CommandMapper();

    // Concurrent collections for managing asynchronous client lifecycles and I/O channels
    private readonly ConcurrentDictionary<int, Task> _clientTasks = new();
    private readonly ConcurrentDictionary<int, Channel<string>> _clientChannels = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly TcpListener _listener;

    private readonly GameEngine _engine;
    private readonly ILogger _logger;
    private readonly int _port;
    private readonly int _maxPlayers;

    // A thread-safe queue containing available player IDs to recycle slots upon disconnection
    private readonly ConcurrentQueue<int> _availablePlayerIds;

    public NetworkServer(GameEngine engine, int port, ILogger logger)
    {
        _engine = engine;
        _logger = logger;
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);

        _maxPlayers = engine.State.Config.MaxPlayers;
        _availablePlayerIds = new ConcurrentQueue<int>(Enumerable.Range(1, _maxPlayers));

        // Subscribe the server to the engine's state change events to trigger broadcasts
        _engine.OnStateChanged += BroadcastState;
    }

    /// <summary>
    /// Starts the TCP listener and the background task for accepting new connections.
    /// </summary>
    public void Start()
    {
        _listener.Start();
        _engine.State.SystemLogs.Notify(new SystemLogData(LogType.System, $"[Server] Started on port {_port}."));
        Task.Run(AcceptClientsAsync);
    }

    /// <summary>
    /// Initiates a graceful shutdown of the server and awaits the termination of all client tasks.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        Task.WhenAll(_clientTasks.Values).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }

    /// <summary>
    /// Continuously listens for incoming TCP client connections.
    /// Enforces the maximum player limit and assigns IDs from the available pool.
    /// </summary>
    private async Task AcceptClientsAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);

                // Reject the connection if the server is at maximum capacity
                if (!_availablePlayerIds.TryDequeue(out int playerId))
                {
                    using var stream = client.GetStream();
                    using var writer = new StreamWriter(stream) { AutoFlush = true };

                    string errorJson = JsonSerializer.Serialize(new { Error = "Server is full. Maximum 9 players allowed." });
                    await writer.WriteLineAsync(errorJson);
                    await writer.FlushAsync();

                    client.Close();
                    continue;
                }

                _engine.RegisterNewPlayer(playerId);

                // Spin up an independent asynchronous task to handle this specific client's lifecycle
                var clientTask = Task.Run(() => HandleClientAsync(client, playerId));
                _clientTasks.TryAdd(playerId, clientTask);
            }
        }
        catch (OperationCanceledException) { /* Expected during graceful shutdown */ }
    }

    /// <summary>
    /// Manages the full lifecycle of a single connected client, including initial handshake,
    /// continuous command reception, and safe disconnection cleanup.
    /// </summary>
    private async Task HandleClientAsync(TcpClient client, int playerId)
    {
        // Set up a bounded channel for outbound messages to prevent memory exhaustion if a client reads slowly
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });
        _clientChannels[playerId] = channel;

        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(stream);
        using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

        // 1. Initial Handshake: Send the static map layout and rules
        var initData = _stateMapper.ExtractInitialData(_engine.State);
        await writer.WriteLineAsync(JsonSerializer.Serialize(initData));
        await writer.FlushAsync();

        // 2. Launch background sender task for broadcasting states
        var senderTask = Task.Run(async () =>
        {
            await foreach (var json in channel.Reader.ReadAllAsync())
            {
                try { await writer.WriteLineAsync(json); }
                catch { break; }
            }
        });

        // Force an immediate broadcast so the new player renders immediately on all clients
        BroadcastState();

        try
        {
            // 3. Continuous Read Loop: Listen for commands from the client
            while (!_cts.Token.IsCancellationRequested)
            {
                string? jsonLine = await reader.ReadLineAsync(_cts.Token);
                if (string.IsNullOrWhiteSpace(jsonLine)) break;

                var commandDto = JsonSerializer.Deserialize<NetworkCommandDTO>(jsonLine);
                if (commandDto != null)
                {
                    ICommand? command = _commandMapper.Map(commandDto);
                    if (command != null)
                    {
                        _engine.EnqueueCommand(playerId, command);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _engine.State.SystemLogs.Notify(new SystemLogData(LogType.System, $"[Server] Player {playerId} error: {ex.Message}"));
        }
        finally
        {
            // 4. Cleanup and Teardown
            channel.Writer.TryComplete();
            await senderTask;

            _clientChannels.TryRemove(playerId, out _);
            _clientTasks.TryRemove(playerId, out _);

            // Notify the domain that the player has disconnected so their avatar is removed
            _engine.DisconnectPlayer(playerId);

            // Return the player ID slot to the available pool for future connections
            _availablePlayerIds.Enqueue(playerId);

            _engine.State.SystemLogs.Notify(new SystemLogData(LogType.System, $"[Server] Player {playerId} slot returned to pool."));
            client.Close();
        }
    }

    /// <summary>
    /// Creates a thread-safe snapshot of the current game state and broadcasts localized 
    /// versions of it to every active client channel.
    /// </summary>
    public void BroadcastState()
    {
        SharedStateSnapshot snapshot;
        lock (_engine.SyncRoot)
        {
            snapshot = _stateMapper.CreateSnapshot(_engine.State, _engine.State.Players, _logger);
        }

        List<int> activeClients = _clientChannels.Keys.ToList();
        foreach (var id in activeClients)
        {
            if (_clientChannels.TryGetValue(id, out var ch))
            {
                try
                {
                    // Build a localized DTO specifically identifying the target client as 'LocalPlayer'
                    var clientDto = _stateMapper.BuildForClient(snapshot, id);
                    string json = JsonSerializer.Serialize(clientDto);
                    ch.Writer.TryWrite(json);
                }
                catch (Exception ex)
                {
                    _engine.State.SystemLogs.Notify(new SystemLogData(LogType.Error, $"[Server] Error broadcasting to Player {id}: {ex.Message}"));
                }
            }
        }
    }
}