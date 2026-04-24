namespace AlchemyRPG;

public interface IKinsmanDeathBehavior
{
    void React(Enemy enemy);
}

public class CowardlyBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy)
    {
        enemy.ModifyAttackDamage(-2);
        GameLogger.Instance.Log(LogType.System, $"{enemy.Name} trembles in fear! Attack decreased.");
    }
}

public class AggressiveBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy)
    {
        enemy.ModifyAttackDamage(3);
        GameLogger.Instance.Log(LogType.System, $"{enemy.Name} rages! Attack increased.");
    }
}

public class NeutralBehavior : IKinsmanDeathBehavior
{
    public void React(Enemy enemy)
    {
    }
}