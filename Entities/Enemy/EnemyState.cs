using System;
using System.Linq;

namespace AlchemyRPG;

public interface IEnemyState
{
    string Name { get; }
    void Update(Enemy enemy, GameState state, Random rand);
    void OnTakeDamage(Enemy enemy, ISubject<SystemLogData> systemLogs, Entity? source);
}

public class AggressiveState : IEnemyState
{
    public string Name => "Aggressive";
    public void Update(Enemy enemy, GameState state, Random rand)
    {
        var player = state.Vision.GetVisiblePlayer(enemy, state);
        if (player != null)
        {
            enemy.ForgetNoise();
            if (enemy.DistanceTo(player.X, player.Y) <= 1) enemy.PerformAttack(player, state);
            else enemy.TryMoveTowards(player.X, player.Y, state);
            return;
        }

        if (enemy.TryGetNoiseTarget(out int targetX, out int targetY))
        {
            if (enemy.X == targetX && enemy.Y == targetY)
            {
                enemy.ForgetNoise();
                enemy.RevertToBaseState("Target reached");
            }
            else if (!enemy.TryMoveTowards(targetX, targetY, state))
            {
                enemy.ForgetNoise();
                enemy.RevertToBaseState("Target unreachable");
            }
            return;
        }
        enemy.RevertToBaseState("No stimuli");
        enemy.MoveRandomly(state, rand);
    }
    public void OnTakeDamage(Enemy enemy, ISubject<SystemLogData> systemLogs, Entity? source) { }
}

public class CowardlyState : IEnemyState
{
    public string Name => "Cowardly";
    public void Update(Enemy enemy, GameState state, Random rand)
    {
        var players = state.Vision.GetVisiblePlayers(enemy, state);
        if (players.Count > 0)
        {
            enemy.ForgetNoise();

            if (state.Vision.IsSurroundedByPlayers(enemy, state, 4)) return;

            if (!enemy.TryMoveSafely(players, state, rand))
                enemy.RevertToBaseState("Cornered");
            return;
        }

        if (enemy.TryGetNoiseTarget(out int targetX, out int targetY))
        {
            if (!enemy.TryMoveAwayFrom(targetX, targetY, state, rand))
            {
                enemy.ForgetNoise();
                enemy.RevertToBaseState("Cannot escape noise");
            }
            return;
        }

        enemy.RevertToBaseState("No stimuli");
        enemy.MoveRandomly(state, rand);
    }
    public void OnTakeDamage(Enemy enemy, ISubject<SystemLogData> systemLogs, Entity? source) { }
}

public class NeutralState : IEnemyState
{
    public string Name => "Neutral";
    private readonly Func<IEnemyState> _enragedStateFactory;
    private readonly Func<IEnemyState> _panickedStateFactory;

    public NeutralState(Func<IEnemyState> enragedStateFactory, Func<IEnemyState> panickedStateFactory)
    {
        _enragedStateFactory = enragedStateFactory;
        _panickedStateFactory = panickedStateFactory;
    }

    public void Update(Enemy enemy, GameState state, Random rand)
    {
        enemy.MoveRandomly(state, rand);
    }

    public void OnTakeDamage(Enemy enemy, ISubject<SystemLogData> systemLogs, Entity? source)
    {
        if (source == null) return;

        var reactionVisitor = new DamageReactionVisitor(enemy, _enragedStateFactory, _panickedStateFactory);
        source.Accept(reactionVisitor);
    }

    private class DamageReactionVisitor : IEntityVisitor
    {
        private readonly Enemy _enemy;
        private readonly Func<IEnemyState> _enragedFactory;
        private readonly Func<IEnemyState> _panickedFactory;

        public DamageReactionVisitor(Enemy enemy, Func<IEnemyState> enragedFactory, Func<IEnemyState> panickedFactory)
        {
            _enemy = enemy;
            _enragedFactory = enragedFactory;
            _panickedFactory = panickedFactory;
        }

        public void VisitPlayer(Player player)
        {
            float hpPercentage = (float)_enemy.Health / _enemy.MaxHealth;
            if (hpPercentage >= 0.5f)
                _enemy.ChangeBaseState(_enragedFactory(), "Attacked by player, HP >= 50% (Enraged)");
            else
                _enemy.ChangeBaseState(_panickedFactory(), "Attacked by player, HP < 50% (Panicked)");
        }

        public void VisitEnemy(Enemy enemy)
        {
        }
    }
}