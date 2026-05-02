namespace AlchemyRPG;

/// <summary>
/// Represents a command to pick up the top-most item located on the player's current tile.
/// </summary>
public class PickUpCommand : ICommand
{
    // Validation 
    public bool CanExecute(GameState state)
    {
        var items = state.Map.GetItemsAt(state.Player.X, state.Player.Y);
        if (items.Count == 0)
        {
            state.Player.LogMessage = "Nothing to pick up here.";
            return false;
        }
        return true;
    }
    /// <summary>
    /// Executes the pick-up action. Searches the ground under the player and triggers 
    /// the item's specific pickup logic if an item is found.
    /// </summary>
    /// <param name="state">The current global state of the game.</param>
    public void Execute(GameState state)
    {
        var items = state.Map.GetItemsAt(state.Player.X, state.Player.Y);
        if (items.Count > 0)
        {
            var item = items.Last();

            item.OnPickUp(state);

            GameLogger.Instance.Log(LogType.Loot, $"{state.Player.Name} picked up {item.Name}");
            state.Log = $"Picked up: {item.Name}";

            if (item.NoiseRange > 0)
            {
                GameLogger.Instance.Log(LogType.Loot, $"Noise generated: {item.NoiseRange}");

                var acousticMap = AcousticSystem.CalculateAcousticDistances(
                    state.Map, state.Player.X, state.Player.Y, item.NoiseRange);

                var noiseData = new NoiseData(state.Player.X, state.Player.Y, acousticMap);
                state.NoiseEvents.Notify(noiseData);
            }
        }
    }
}