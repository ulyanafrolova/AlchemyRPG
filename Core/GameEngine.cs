using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AlchemyRPG;

/// <summary>
/// The authoritative core engine of the game.
/// Manages the global state, processes queued player commands, and drives the real-time game loop.
/// </summary>
public class GameEngine
{
    /// <summary>
    /// Gets the current authoritative state of the game world.
    /// </summary>
    public GameState State { get; }

    private readonly object _syncRoot = new();

    /// <summary>
    /// Gets the synchronization object used to lock the domain state for safe multi-threaded access.
    /// </summary>
    public object SyncRoot => _syncRoot;

    /// <summary>
    /// Thread-safe buffers holding incoming commands for each connected player.
    /// </summary>
    private readonly ConcurrentDictionary<int, ConcurrentQueue<ICommand>> _playerCommandBuffers = new();

    /// <summary>
    /// Event triggered whenever the game state is modified, signaling the network layer to broadcast updates.
    /// </summary>
    public event Action? OnStateChanged;

    private const int TurnTimeoutMs = 1000;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    /// <param name="state">The initial game state constructed by the factory.</param>
    /// <param name="logger">The system logger for recording engine events.</param>
    public GameEngine(GameState state, ILogger logger)
    {
        State = state;
        _logger = logger;
    }

    /// <summary>
    /// Starts the asynchronous game loop in a background task.
    /// </summary>
    public void Start()
    {
        Task.Run(WorldTickAsync);
    }

    /// <summary>
    /// Safely invokes the state change event to notify external observers (e.g., the Network Server).
    /// </summary>
    private void TriggerStateChange()
    {
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Spawns and registers a new player into the active game session.
    /// </summary>
    /// <param name="playerId">The unique identifier of the connecting player.</param>
    public void RegisterNewPlayer(int playerId)
    {
        lock (_syncRoot)
        {
            var spawn = State.Map.GetRandomWalkableTile(Random.Shared);
            if (!spawn.HasValue) return;

            var newPlayer = new Player($"Player {playerId}", spawn.Value.x, spawn.Value.y);
            newPlayer.InitializeHearing(State.NoiseEvents, State.PlayerHeardNoiseEvents);

            State.TryAddPlayer(playerId, newPlayer);
            _playerCommandBuffers.TryAdd(playerId, new ConcurrentQueue<ICommand>());

            State.EventLog.Push($"Player {playerId} joined the game.");
            State.SystemLogs.Notify(new SystemLogData(LogType.System, $"Player {playerId} joined the game."));
        }
        TriggerStateChange();
    }

    /// <summary>
    /// Removes a disconnecting player from the game session and cleans up their resources.
    /// </summary>
    /// <param name="playerId">The unique identifier of the disconnecting player.</param>
    public void DisconnectPlayer(int playerId)
    {
        lock (_syncRoot)
        {
            if (State.TryRemovePlayer(playerId, out Player? disconnectingPlayer))
            {
                disconnectingPlayer?.TeardownHearing(State.NoiseEvents);
                State.EventLog.Push($"Player {playerId} left the game.");
                State.SystemLogs.Notify(new SystemLogData(LogType.System, $"Player {playerId} left the game."));
            }
            _playerCommandBuffers.TryRemove(playerId, out _);
        }
        TriggerStateChange();
    }

    /// <summary>
    /// Enqueues a validated domain command to be processed during the next server tick.
    /// </summary>
    /// <param name="playerId">The identifier of the player issuing the command.</param>
    /// <param name="command">The command to execute.</param>
    public void EnqueueCommand(int playerId, ICommand command)
    {
        if (_playerCommandBuffers.TryGetValue(playerId, out var buffer))
        {
            buffer.Enqueue(command);
        }
    }

    /// <summary>
    /// The main real-time game loop that continuously processes inputs and manages time
    /// </summary>
    private async Task WorldTickAsync()
    {
        // Ensures immediate reaction to player commands without burning CPU resources.
        const int ServerTickRateMs = 100;

        const int EnemyUpdateIntervalMs = 1000;
        int enemyTickAccumulator = 0;

        while (true)
        {
            try
            {
                bool stateChanged = false;
                bool enemiesNeedUpdate = false;
                // Accumulate time delta for independent updates
                enemyTickAccumulator += ServerTickRateMs;
                if (enemyTickAccumulator >= EnemyUpdateIntervalMs)
                {
                    enemiesNeedUpdate = true;
                    enemyTickAccumulator = 0;
                }
                lock (_syncRoot)
                {
                    if (State.IsGameOver)
                    {
                        // Handle game over logic here if needed
                    }
                    else
                    {
                        // Process all queued player commands
                        foreach (var kvp in State.Players)
                        {
                            int pid = kvp.Key;
                            Player player = kvp.Value;
                            player.ClearLogMessage();

                            if (_playerCommandBuffers.TryGetValue(pid, out var queue))
                            {
                                while (queue.TryDequeue(out ICommand? command))
                                {
                                    if (command.CanExecute(State, player))
                                    {
                                        command.Execute(State, player);
                                        stateChanged = true;
                                    }
                                }
                            }
                        }
                        // Process entity updates 
                        if (enemiesNeedUpdate)
                        {
                            foreach (var enemy in State.Map.Enemies.ToList())
                            {
                                if (!enemy.IsDead)
                                {
                                    enemy.Update(State, Random.Shared);
                                    stateChanged = true;
                                }
                            }
                        }
                    }
                }

                // Broadcast updates if the world state was modified
                if (stateChanged)
                {
                    TriggerStateChange();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, $"[CRITICAL ENGINE FAILURE AVOIDED] {ex.Message}\n{ex.StackTrace}");
            }
            // The server yields a time quantum to the OS thread pool and wakes up for the next tick
            await Task.Delay(ServerTickRateMs);
        }
    }
}