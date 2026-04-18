namespace AlchemyRPG;

/// <summary>
/// Abstract base class for all living entities on the map (Player, Enemies, NPCs).
/// Holds shared attributes like coordinates, health, and rendering symbols.
/// </summary>
public abstract class Entity
{
    public string Name { get; protected set; }
    public char Symbol { get; protected set; }

    public int Health { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    protected Entity(string name, char symbol, int health)
    {
        Name = name;
        Symbol = symbol;
        Health = health;
    }
}