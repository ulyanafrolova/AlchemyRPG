using System;

namespace AlchemyRPG;

/// <summary>
/// Implements the strictly typed IObserver<T> interfaces.
/// </summary>
public class Enemy : Entity, IObserver<NoiseData>, IObserver<EnemyDeathData>
{
    public string Species { get; }
    public int AttackDamage { get; private set; }
    public int Armor { get; }

    private readonly IKinsmanDeathBehavior _deathBehavior;

    private readonly Subject<NoiseData> _noiseEvents;
    private readonly Subject<EnemyDeathData> _deathEvents;

    public Enemy(
        string name,
        string species,
        int health,
        int attackDamage,
        int armor,
        Subject<NoiseData> noiseEvents,
        Subject<EnemyDeathData> deathEvents,
        IKinsmanDeathBehavior? deathBehavior = null)
        : base(name, Tiles.Enemy, health)
    {
        Species = species;
        AttackDamage = attackDamage;
        Armor = armor;

        _noiseEvents = noiseEvents;
        _deathEvents = deathEvents;

        _deathBehavior = deathBehavior ?? new NeutralBehavior();

        _noiseEvents.Subscribe(this);
        _deathEvents.Subscribe(this);
    }

    public void ModifyAttackDamage(int delta)
    {
        AttackDamage = Math.Max(0, AttackDamage + delta);
    }

    private void ReactToKinsmanDeath()
    {
        _deathBehavior.React(this);
    }

    public void OnNext(NoiseData noise)
    {
        if (noise.ReachedTiles.TryGetValue((this.X, this.Y), out int distanceToSource))
        {
            GameLogger.Instance.Log(LogType.System,
                $"[{Species} at {X},{Y}] Heard a noise from {noise.SourceX},{noise.SourceY} (Distance: {distanceToSource} steps).");
        }
    }

    public void OnNext(EnemyDeathData deathInfo)
    {
        if (deathInfo.Species == this.Species)
        {
            ReactToKinsmanDeath();
        }
    }

    /// <summary>
    /// Safely prepares the enemy for removal from the game world.
    /// </summary>
    public void TriggerDeathProcessing()
    {
        _deathEvents.Notify(new EnemyDeathData(this.Species));

        _noiseEvents.Unsubscribe(this);
        _deathEvents.Unsubscribe(this);
    }

    public void Update(GameState state, Random rand)
    {
        if (IsEngagedInCombat(state.Player)) return;
        MoveRandomly(state.Map, rand);
    }

    private bool IsEngagedInCombat(Player player)
    {
        return Math.Abs(this.X - player.X) <= 1 && Math.Abs(this.Y - player.Y) <= 1;
    }

    private void MoveRandomly(Map map, Random rand)
    {
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        int direction = rand.Next(4);
        int newX = X + dx[direction];
        int newY = Y + dy[direction];

        if (newX >= 0 && newX < map.Width && newY >= 0 && newY < map.Height)
        {
            if (map.IsWalkable(newX, newY))
            {
                SetPosition(newX, newY);
            }
        }
    }

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