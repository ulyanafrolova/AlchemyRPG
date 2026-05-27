namespace AlchemyRPG;

/// <summary>
/// Defines the strategy for how an enemy reacts when another enemy of the same species dies.
/// Implements the Strategy design pattern.
/// </summary>
public interface IKinsmanDeathBehavior
{
    /// <summary>
    /// Executes the specific reaction behavior.
    /// </summary>
    /// <param name="enemy">The enemy reacting to the event.</param>
    /// <param name="systemLogs">The system logger bus for recording the reaction.</param>
    void React(Enemy enemy, ISubject<SystemLogData> systemLogs);
}

/// <summary>
/// A behavior strategy where the enemy loses morale and becomes weaker when a kinsman dies.
/// </summary>
public class CowardlyBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy, ISubject<SystemLogData> systemLogs)
    {
        enemy.ModifyAttackDamage(-2);
        systemLogs.Notify(new SystemLogData(LogType.Combat,
            $"[ModifierApplied] Target:{enemy.Name}, Attribute:Attack, Delta:-2, Reason:KinsmanDeath_Cowardly"));
    }
}

/// <summary>
/// A behavior strategy where the enemy becomes enraged and stronger when a kinsman dies.
/// </summary>
public class AggressiveBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy, ISubject<SystemLogData> systemLogs)
    {
        enemy.ModifyAttackDamage(3);
        systemLogs.Notify(new SystemLogData(LogType.Combat,
            $"[ModifierApplied] Target:{enemy.Name}, Attribute:Attack, Delta:+3, Reason:KinsmanDeath_Aggressive"));
    }
}

/// <summary>
/// A behavior strategy where the enemy does not react to the death of a kinsman.
/// </summary>
public class NeutralBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy, ISubject<SystemLogData> systemLogs) 
    { 
        // No reaction; enemy remains unchanged.
    }
}