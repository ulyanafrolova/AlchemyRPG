using System;

namespace AlchemyRPG;

/// <summary>
/// Abstract base class for all living entities on the map (e.g., Players, Enemies, NPCs).
/// Holds shared domain attributes like coordinates and health state.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets or sets the display name of the entity.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the current health points of the entity.
    /// </summary>
    public int Health { get; private set; }

    /// <summary>
    /// Gets the horizontal coordinate (X-axis) of the entity on the map.
    /// </summary>
    public int X { get; protected set; }

    /// <summary>
    /// Gets the vertical coordinate (Y-axis) of the entity on the map.
    /// </summary>
    public int Y { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the entity's health has reached zero.
    /// </summary>
    public bool IsDead => Health <= 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    protected Entity(string name, int health)
    {
        Name = name;
        Health = health;
    }

    /// <summary>
    /// Instantly moves the entity to the specified coordinates.
    /// Validates the destination against the map's collision rules.
    /// </summary>
    public void Teleport(int x, int y, Map map)
    {
        if (!map.IsWalkable(x, y))
            throw new InvalidOperationException($"[Architecture Violation] Attempt to place entity at unwalkable coordinates ({x}, {y}).");

        X = x;
        Y = y;
    }

    /// <summary>
    /// Reduces the entity's health by the specified amount. Ensures health does not drop below zero.
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
    }
}