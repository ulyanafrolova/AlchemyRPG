using System;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents a hostile non-player character in the game world.
/// Implements the strictly typed Observer interfaces to react to environmental noises and the deaths of other enemies.
/// </summary>
public class Enemy : Entity, IObserver<NoiseData>, IObserver<EnemyDeathData>
{
    private static readonly int[] Dx = { 0, 0, -1, 1 };
    private static readonly int[] Dy = { -1, 1, 0, 0 };
    private readonly ISubject<SystemLogData> _systemLogs;
    
    /// <summary>
    /// Gets the species or classification of the enemy (e.g., "Goblin", "Skeleton").
    /// Used to determine group behavior and reactions to kinsman deaths.
    /// </summary>
    public string Species { get; }
    
    /// <summary>
    /// Gets the current attack damage of the enemy. This value can be modified dynamically.
    /// </summary>
    public int AttackDamage { get; private set; }
    
    /// <summary>
    /// Gets the innate armor value of the enemy, which reduces incoming damage.
    /// </summary>
    public int Armor { get; }

    private readonly IKinsmanDeathBehavior _deathBehavior;
    private readonly ISubject<NoiseData> _noiseEvents;
    private readonly ISubject<EnemyDeathData> _deathEvents;
    private readonly ISubject<EnemyHeardNoiseData> _heardNoiseEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="Enemy"/> class.
    /// </summary>
    public Enemy(
        string name,
        string species,
        int health,
        int attackDamage,
        int armor,
        ISubject<NoiseData> noiseEvents,
        ISubject<EnemyDeathData> deathEvents,
        ISubject<EnemyHeardNoiseData> heardNoiseEvents,
        ISubject<SystemLogData> systemLogs,
        IKinsmanDeathBehavior? deathBehavior = null)
        : base(name, health)
    {
        Species = species;
        AttackDamage = attackDamage;
        Armor = armor;

        _noiseEvents = noiseEvents;
        _deathEvents = deathEvents;
        _heardNoiseEvents = heardNoiseEvents;
        _deathBehavior = deathBehavior ?? new NeutralBehavior();
        _systemLogs = systemLogs;
        
        // Subscribe to relevant domain events upon creation
        _noiseEvents.Subscribe(this);
        _deathEvents.Subscribe(this);
    }

    /// <summary>
    /// Handles incoming noise events. If the noise reaches the enemy's coordinates, 
    /// it notifies the system that the enemy has detected the sound.
    /// </summary>
    public void OnNext(NoiseData noise)
    {
        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distanceToSource))
        {
            _heardNoiseEvents.Notify(new EnemyHeardNoiseData(Species, X, Y, noise.SourceX, noise.SourceY, distanceToSource));
        }
    }

    /// <summary>
    /// The primary update loop, invoked periodically by the GameEngine.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="rand">The random number generator instance.</param>
    public void Update(GameState state, Random rand)
    {
        if (IsEngagedInCombat(state)) return;
        MoveRandomly(state, rand);
    }

    /// <summary>
    /// Checks if the enemy is adjacent to any active player, preventing them from moving if engaged in melee.
    /// </summary>
    private bool IsEngagedInCombat(GameState state)
    {
        return state.GetAllActivePlayers().Any(p =>
            Math.Abs(this.X - p.X) <= 1 && Math.Abs(this.Y - p.Y) <= 1);
    }

    /// <summary>
    /// Executes a random movement in one of the four cardinal directions, checking for walls and player collisions.
    /// </summary>
    private void MoveRandomly(GameState state, Random rand)
    {
        int direction = rand.Next(4);
        int newX = X + Dx[direction];
        int newY = Y + Dy[direction];

        var map = state.Map;
        if (newX >= 0 && newX < map.Width && newY >= 0 && newY < map.Height)
        {
            if (map.IsWalkable(newX, newY))
            {
                bool tileOccupiedByPlayer = state.GetAllActivePlayers()
                                                 .Any(p => p.X == newX && p.Y == newY);
                if (!tileOccupiedByPlayer)
                {
                    Teleport(newX, newY, state.Map);
                }
            }
        }
    }

    /// <summary>
    /// Dynamically modifies the enemy's attack damage (e.g., as a result of a behavioral reaction).
    /// </summary>
    public void ModifyAttackDamage(int delta)
    {
        AttackDamage = Math.Max(0, AttackDamage + delta);
    }

    /// <summary>
    /// Triggers the specific behavioral response when an enemy of the same species dies.
    /// </summary>
    private void ReactToKinsmanDeath()
    {
        _deathBehavior.React(this, _systemLogs);
    }

    /// <summary>
    /// Handles the event when any enemy dies on the map.
    /// </summary>
    public void OnNext(EnemyDeathData deathInfo)
    {
        if (deathInfo.Species == this.Species)
        {
            ReactToKinsmanDeath();
        }
    }

    /// <summary>
    /// Safely prepares the enemy for removal from the game world by unsubscribing from all events
    /// and notifying its kinsmen of its demise.
    /// </summary>
    public void TriggerDeathProcessing()
    {
        _noiseEvents.Unsubscribe(this);
        _deathEvents.Unsubscribe(this);
        _deathEvents.Notify(new EnemyDeathData(this.Species));
    }

    /// <summary>
    /// Applies damage to the enemy's health pool and triggers death processing if health drops to zero.
    /// </summary>
    public override void TakeDamage(int amount)
    {
        bool wasDead = IsDead;
        base.TakeDamage(amount);

        if (IsDead && !wasDead)
        {
            TriggerDeathProcessing();
        }
    }
}