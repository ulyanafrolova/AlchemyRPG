namespace AlchemyRPG;

/// <summary>
/// Represents premium currency found on the map. 
/// It is instantly consumed upon pickup and does not occupy inventory space.
/// </summary>
/// <param name="amount">The amount of gold this item grants.</param>
public class Gold(int amount) : IItem
{
    public string Name => "Gold";
    public char Symbol => Tiles.Gold;
    private readonly int _amount = amount;

    public void OnPickUp(GameState state)
    {
        state.Player.Gold += _amount;
        state.Log = $"Picked up {_amount} gold.";
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
    }
}

/// <summary>
/// Represents standard currency found on the map.
/// It is instantly consumed upon pickup and does not occupy inventory space.
/// </summary>
/// <param name="amount">The amount of coins this item grants.</param>
public class Coin(int amount) : IItem
{
    public string Name => "Coin";
    public char Symbol => Tiles.Coin;
    private readonly int _amount = amount;

    public void OnPickUp(GameState state)
    {
        state.Player.Coins += _amount;
        state.Log = $"Picked up {_amount} coins.";
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
    }
}