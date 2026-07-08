using System;
using System.Collections.Generic;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents the authoritative domain entity for a non-player character (Enemy).
/// </summary>
/// <remarks>
/// Acts as a complex Aggregate Root combining multiple behavioral patterns:
/// - State Pattern (<see cref="IEnemyState"/>) for dynamic AI decision-making.
/// - Strategy Pattern (<see cref="IKinsmanDeathBehavior"/>) for reactive morale systems.
/// - Observer Pattern (<see cref="IObserver{T}"/>) for asynchronous acoustic and domain event processing.
/// </remarks>
public class Enemy : Entity, IObserver<NoiseData>, IObserver<EnemyDeathData>
{
    // Cardinal direction vectors (Up, Down, Left, Right) used for grid-based spatial calculations.
    private static readonly int[] Dx = { 0, 0, -1, 1 };
    private static readonly int[] Dy = { -1, 1, 0, 0 };

    /// <summary>
    /// The default temperament of the enemy to which it reverts when no active stimuli are present.
    /// </summary>
    private IEnemyState _baseState;

    private readonly ISubject<SystemLogData> _systemLogs;
    private readonly IKinsmanDeathBehavior _deathBehavior;

    // Event bus subscriptions. Tracked internally to ensure safe disposal upon entity death.
    private readonly ISubject<NoiseData> _noiseEvents;
    private readonly ISubject<EnemyDeathData> _deathEvents;
    private readonly ISubject<EnemyHeardNoiseData> _heardNoiseEvents;

    public string Species { get; }

    private readonly int _baseAttackDamage;
    private int _attackDamageModifier = 0;

    /// <summary>
    /// Gets the dynamically calculated combat damage, ensuring it never drops below zero.
    /// </summary>
    public int AttackDamage => Math.Max(0, _baseAttackDamage + _attackDamageModifier);
    public int Armor { get; }

    public int MaxHealth { get; }

    /// <summary>
    /// A localized spatial memory of the last acoustic stimulus.
    /// </summary>
    private (int X, int Y)? _lastHeardNoise;

    public void HearNoise(int x, int y) => _lastHeardNoise = (x, y);
    public void ForgetNoise() => _lastHeardNoise = null;

    private IEnemyState _currentState;

    /// <summary>
    /// Permanently alters the foundational temperament of the enemy and immediately transitions to it.
    /// </summary>
    public void ChangeBaseState(IEnemyState newBaseState, string reason = "Permanent temperament shift")
    {
        _baseState = newBaseState;
        ChangeState(newBaseState, reason);
    }

    public Enemy(
        string name, string species, int health, int attackDamage, int armor,
        ISubject<NoiseData> noiseEvents, ISubject<EnemyDeathData> deathEvents,
        ISubject<EnemyHeardNoiseData> heardNoiseEvents, ISubject<SystemLogData> systemLogs,
        IEnemyState initialState, IKinsmanDeathBehavior? deathBehavior = null)
        : base(name, health)
    {
        Species = species;
        _baseAttackDamage = attackDamage;
        Armor = armor;
        MaxHealth = health;

        _noiseEvents = noiseEvents;
        _deathEvents = deathEvents;
        _heardNoiseEvents = heardNoiseEvents;
        _systemLogs = systemLogs;

        _baseState = initialState;
        _currentState = initialState;
        _deathBehavior = deathBehavior ?? new NeutralBehavior();

        // Establish structural coupling to the domain event buses.
        _noiseEvents.Subscribe(this);
        _deathEvents.Subscribe(this);
    }
    public void RevertToBaseState(string reason)
    {
        ChangeState(_baseState, reason);
    }
    public void ChangeState(IEnemyState newState, string reason = "Logic transition")
    {
        if (_currentState != null && _currentState.Name == newState.Name)
            return;

        _currentState = newState;
        _systemLogs.Notify(new SystemLogData(LogType.System, $"{Name} changed state to {newState.Name}. Reason: {reason}"));
    }

    /// <summary>
    /// The primary execution tick for the AI entity. Delegates decision-making to the active state strategy.
    /// </summary>
    public void Update(GameState state, Random rand)
    {
        if (IsDead) return;
        _currentState.Update(this, state, rand);
    }

    /// <summary>
    /// Handles asynchronous acoustic stimulus.
    /// </summary>
    public void OnNext(NoiseData noise)
    {
        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distanceToSource))
        {
            HearNoise(noise.SourceX, noise.SourceY);
            _heardNoiseEvents.Notify(new EnemyHeardNoiseData(Species, X, Y, noise.SourceX, noise.SourceY, distanceToSource));
        }
    }

    /// <summary>
    /// Processes incoming damage and triggers state-specific reactions or structural teardown upon death.
    /// </summary>
    public override void TakeDamage(int amount, Entity? source = null)
    {
        if (IsDead) return;
        bool wasDead = IsDead;

        base.TakeDamage(amount, source);

        if (!IsDead)
        {
            _currentState.OnTakeDamage(this, _systemLogs, source);
        }
        else if (IsDead && !wasDead)
        {
            TriggerDeathProcessing();
        }
    }
    public int DistanceTo(int x, int y) => Math.Abs(X - x) + Math.Abs(Y - y);

    /// <summary>
    /// Attempts to find a movement vector that increases the distance from all known threats.
    /// </summary>
    /// <remarks>
    /// Evaluates adjacent tiles against all provided threats in O(T) time where T is the number of threats.
    /// Does not utilize advanced pathfinding (e.g., A* or Dijkstra), acting strictly on local adjacency analysis.
    /// </remarks>
    public bool TryMoveSafely(List<Player> threats, GameState state, Random rand)
    {
        var validMoves = new List<(int dx, int dy)>();

        for (int i = 0; i < 4; i++)
        {
            int nx = X + Dx[i];
            int ny = Y + Dy[i];

            if (!state.Map.IsWalkable(nx, ny) || state.GetAllActivePlayers().Any(p => p.X == nx && p.Y == ny))
                continue;

            bool isSafe = true;
            foreach (var threat in threats)
            {
                // Verify that moving to the adjacent tile does not strictly decrease the distance to the threat.
                if ((Math.Abs(nx - threat.X) + Math.Abs(ny - threat.Y)) < DistanceTo(threat.X, threat.Y))
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
                validMoves.Add((Dx[i], Dy[i]));
        }

        if (validMoves.Count > 0)
        {
            var move = validMoves[rand.Next(validMoves.Count)];
            Teleport(X + move.dx, Y + move.dy, state.Map);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes a greedy, single-step movement towards a specified target coordinate.
    /// Prioritizes movement along the axis with the greatest distance delta.
    /// </summary>
    public bool TryMoveTowards(int targetX, int targetY, GameState state)
    {
        int dx = Math.Sign(targetX - X);
        int dy = Math.Sign(targetY - Y);

        if (Math.Abs(targetX - X) > Math.Abs(targetY - Y))
            return (dx != 0 && TryMove(dx, 0, state)) || (dy != 0 && TryMove(0, dy, state));
        else
            return (dy != 0 && TryMove(0, dy, state)) || (dx != 0 && TryMove(dx, 0, state));
    }

    /// <summary>
    /// Analyzes adjacent valid tiles and executes a move to the tile that maximizes distance from the target.
    /// </summary>
    public bool TryMoveAwayFrom(int targetX, int targetY, GameState state, Random rand)
    {
        var validMoves = new List<(int nx, int ny, int dist)>();

        for (int i = 0; i < 4; i++)
        {
            int nx = X + Dx[i];
            int ny = Y + Dy[i];

            if (state.Map.IsWalkable(nx, ny) && !state.GetAllActivePlayers().Any(p => p.X == nx && p.Y == ny))
            {
                int distToTarget = Math.Abs(nx - targetX) + Math.Abs(ny - targetY);
                validMoves.Add((nx, ny, distToTarget));
            }
        }

        var bestMoves = validMoves.OrderByDescending(m => m.dist).ToList();

        if (bestMoves.Count > 0)
        {
            var optimalDist = bestMoves.First().dist;
            var optimalMoves = bestMoves.Where(m => m.dist == optimalDist).ToList();
            var chosenMove = optimalMoves[rand.Next(optimalMoves.Count)];

            Teleport(chosenMove.nx, chosenMove.ny, state.Map);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates and applies a localized movement vector if the destination is unobstructed.
    /// </summary>
    private bool TryMove(int dx, int dy, GameState state)
    {
        int nx = X + dx;
        int ny = Y + dy;

        if (state.Map.IsWalkable(nx, ny) && !state.GetAllActivePlayers().Any(p => p.X == nx && p.Y == ny))
        {
            Teleport(nx, ny, state.Map);
            return true;
        }
        return false;
    }

    public void MoveRandomly(GameState state, Random rand)
    {
        int dir = rand.Next(4);
        TryMove(Dx[dir], Dy[dir], state);
    }

    /// <summary>
    /// Executes a combat action against the specified player, calculating effective damage against player evasion metrics.
    /// </summary>
    public void PerformAttack(Player player, GameState state)
    {
        int damage = Math.Max(0, AttackDamage - player.Dexterity);
        player.TakeDamage(damage, this);

        state.SystemLogs.Notify(new SystemLogData(LogType.Combat, $"{Name} hit {player.Name} for {damage} dmg!"));
        state.EventLog.Push($"{Name} bit {player.Name} for {damage}!");

        if (player.IsDead)
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.System, $"{player.Name} was killed by {Name}."));
            state.EventLog.Push($"[!] {player.Name} was killed by {Name}!");
        }
    }

    public void AddDamageModifier(int delta)
    {
        _attackDamageModifier += delta;
    }

    /// <summary>
    /// Invoked structurally when any enemy dies within the domain. 
    /// Delegates logic to the strategy behavior if the deceased shares the same species.
    /// </summary>
    public void OnNext(EnemyDeathData deathInfo)
    {
        if (deathInfo.Species == this.Species) _deathBehavior.React(this, _systemLogs);
    }

    /// <summary>
    /// Disconnects the entity from global domain buses.
    /// Failure to call this upon destruction will result in memory leaks and ghost-processing by dead entities.
    /// </summary>
    public void TriggerDeathProcessing()
    {
        _noiseEvents.Unsubscribe(this);
        _deathEvents.Unsubscribe(this);
        _deathEvents.Notify(new EnemyDeathData(Species));
    }

    public override void Accept(IEntityVisitor visitor) => visitor.VisitEnemy(this);

    /// <summary>
    /// Attempts to extract the coordinates of the most recently perceived acoustic event.
    /// </summary>
    /// <returns>True if a valid sound signature is held in memory; otherwise, false.</returns>
    public bool TryGetNoiseTarget(out int x, out int y)
    {
        if (_lastHeardNoise.HasValue)
        {
            x = _lastHeardNoise.Value.X;
            y = _lastHeardNoise.Value.Y;
            return true;
        }
        x = 0; y = 0;
        return false;
    }
}