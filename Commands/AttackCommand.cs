using System;
using System.Linq;

namespace AlchemyRPG;

/// <summary>
/// Represents a command that executes a combat action against a target at specific coordinates.
/// Handles validation, damage calculation using the Visitor pattern, and logging for both the attacker and the defender.
/// </summary>
public class AttackCommand : ICommand
{
    private readonly int _targetX;
    private readonly int _targetY;
    private readonly Func<Player, AttackVisitor> _visitorFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackCommand"/> class.
    /// </summary>
    /// <param name="targetX">The X coordinate of the target to attack.</param>
    /// <param name="targetY">The Y coordinate of the target to attack.</param>
    /// <param name="attackType">The specific type of attack to perform (e.g., Normal, Stealth, Magic).</param>
    public AttackCommand(int targetX, int targetY, Func<Player, AttackVisitor> visitorFactory)
    {
        _targetX = targetX;
        _targetY = targetY;
        _visitorFactory = visitorFactory;
    }

    /// <summary>
    /// Validates whether the attack can be performed. 
    /// Ensures the player is alive and that the target is within a valid melee range (1 tile in any direction).
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player attempting to execute the command.</param>
    /// <returns>True if the attack is valid and allowed; otherwise, false.</returns>
    public bool CanExecute(GameState state, Player executor)
    {
        if (executor.IsDead) return false;

        int distanceX = Math.Abs(executor.X - _targetX);
        int distanceY = Math.Abs(executor.Y - _targetY);
        int currentRange = executor.AttackRange;

        // Attacks are strictly limited to adjacent tiles
        if (distanceX > currentRange || distanceY > currentRange)
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.System, $"[SECURITY ALERT] Player {executor.Name} attempted to attack out of range at ({_targetX}, {_targetY})."));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Executes the attack against an enemy at the target coordinates.
    /// Processes the combat exchange, updates entity health, logs the outcome, and handles death events.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    /// <param name="executor">The player executing the command.</param>
    public void Execute(GameState state, Player executor)
    {
        var enemy = state.Map.GetEnemyAt(_targetX, _targetY);
        if (enemy == null)
        {
            state.EventLog.Push($"{executor.Name} swings at the empty air.");
            return;
        }

        AttackResult result = PerformCombatExchange(executor, enemy, _visitorFactory);
        state.SystemLogs.Notify(new SystemLogData(LogType.Combat, $"{executor.Name} dealt {result.DamageDealt} dmg to {enemy.Name}."));

        if (result.IsEnemyDead)
        {
            state.Map.RemoveEnemy(enemy);
            state.EventLog.Push($"{executor.Name} killed {enemy.Name} with {result.DamageDealt} dmg!");
            state.SystemLogs.Notify(new SystemLogData(LogType.Combat, $"{enemy.Name} died in combat."));
            return;
        }

        state.SystemLogs.Notify(new SystemLogData(LogType.Combat, $"{executor.Name} took {result.DamageTaken} dmg from {enemy.Name}."));
        state.EventLog.Push($"{executor.Name} hit for {result.DamageDealt}. {enemy.Name} hits back for {result.DamageTaken}!");

        if (result.IsPlayerDead)
        {
            state.SystemLogs.Notify(new SystemLogData(LogType.System, $"{executor.Name} died in combat."));
            state.EventLog.Push($"{executor.Name} died in combat!");
        }
    }

    /// <summary>
    /// Orchestrates the mathematical exchange of damage between the player and the enemy.
    /// Uses the Visitor pattern to dynamically calculate damage based on the equipped weapon type.
    /// </summary>
    /// <param name="executor">The player initiating the attack.</param>
    /// <param name="enemy">The enemy defending against the attack.</param>
    /// <param name="attackType">The combat style chosen by the player.</param>
    /// <returns>An <see cref="AttackResult"/> containing the calculated damage and survival status.</returns>
    private static AttackResult PerformCombatExchange(Player executor, Enemy enemy, Func<Player, AttackVisitor> visitorFactory)
    {
        IInventoryItem? activeItem = executor.RightHand ?? executor.LeftHand;
        var visitor = visitorFactory(executor);

        if (activeItem != null) activeItem.Accept(visitor);
        else visitor.VisitNonWeapon();

        int damageDealt = Math.Max(0, visitor.CalculatedDamage - enemy.Armor);
        enemy.TakeDamage(damageDealt, executor);

        int damageTaken = 0;
        if (!enemy.IsDead)
        {
            damageTaken = Math.Max(0, enemy.AttackDamage - visitor.CalculatedDefense);
            executor.TakeDamage(damageTaken, enemy);
        }

        return new AttackResult(damageDealt, damageTaken, enemy.IsDead, executor.IsDead);
    }
}