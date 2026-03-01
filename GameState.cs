namespace AlchemyRPG;

public class GameState
{
    public required Player Player { get; set; }
    public required Map Map { get; set; }
    public string Log { get; set; } = "Welcome to the game!";
}