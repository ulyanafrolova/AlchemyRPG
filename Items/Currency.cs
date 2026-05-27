namespace AlchemyRPG;

/// <summary>
/// Represents premium currency found on the map. 
/// It is instantly consumed upon pickup and does not occupy inventory space.
/// </summary>
/// <param name="amount">The amount of gold this item grants.</param>
public class Gold(int amount) : IItem
{
    public string Name => "Gold";
    private readonly int _amount = amount;
    public Guid Id { get; } = Guid.NewGuid();

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddGold(_amount);
        state.EventLog.Push($"Picked up {_amount} gold.");
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }
    public T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitGold(this);
}

/// <summary>
/// Represents standard currency found on the map.
/// It is instantly consumed upon pickup and does not occupy inventory space.
/// </summary>
/// <param name="amount">The amount of coins this item grants.</param>
public class Coin(int amount) : IItem
{
    public string Name => "Coin";
    private readonly int _amount = amount;

    public Guid Id { get; } = Guid.NewGuid();

    public void OnPickUp(GameState state, Player executor)
    {
        executor.AddCoins(_amount);
        state.EventLog.Push($"{executor.Name} picked up {_amount} coins.");
        state.SystemLogs.Notify(new SystemLogData(LogType.Loot, $"{executor.Name} picked up {_amount} coins."));
        state.Map.RemoveItem(executor.X, executor.Y, this);
    }
    public T Accept<T>(IItemVisitor<T> visitor) => visitor.VisitCoin(this);
}