namespace AlchemyRPG;

public interface ITile 
{
    bool IsWalkable { get; }
}

public class FloorTile : ITile 
{
    public bool IsWalkable => true;
}

public class WallTile : ITile 
{
    public bool IsWalkable => false;
}