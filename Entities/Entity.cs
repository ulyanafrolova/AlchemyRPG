namespace AlchemyRPG;

/// <summary>
/// Abstract base class for all living entities on the map (Player, Enemies, NPCs).
/// Holds shared attributes like coordinates, health, and rendering symbols.
/// </summary>
public abstract class Entity
{
    public string Name { get; protected set; }
    public char Symbol { get; protected set; }

    public int Health { get; private set; }
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public bool IsDead => Health <= 0;

    protected Entity(string name, char symbol, int health)
    {
        Name = name;
        Symbol = symbol;
        Health = health;
    }
    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
    public virtual void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
    }
}