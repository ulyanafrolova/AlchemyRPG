namespace AlchemyRPG;

public class NoiseData
{
    public int SourceX { get; }
    public int SourceY { get; }
    public Dictionary<(int x, int y), int> ReachedTiles { get; }

    public NoiseData(int sourceX, int sourceY, Dictionary<(int x, int y), int> reachedTiles)
    {
        SourceX = sourceX;
        SourceY = sourceY;
        ReachedTiles = reachedTiles;
    }
}

public class EnemyDeathData
{
    public string Species { get; }

    public EnemyDeathData(string species)
    {
        Species = species;
    }
}