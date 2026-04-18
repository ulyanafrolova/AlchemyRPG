namespace AlchemyRPG;

/// <summary>
/// Implements the <see cref="IItem"/> interface to allow integration with the 
/// existing map grid system, meaning enemies occupy tiles just like items, 
/// but trigger combat interactions instead of being added to the inventory.
/// </summary>
public class Enemy : Entity
{
    /// <summary>
    /// Gets the base attack power of the enemy. 
    /// </summary>
    public int AttackDamage { get; }
    /// <summary>
    /// Gets the defensive armor rating of the enemy.
    /// </summary>
    public int Armor { get; }
    public Enemy(string name, int health, int attackDamage, int armor)
        : base(name, Tiles.Enemy, health) 
    {
        AttackDamage = attackDamage;
        Armor = armor;
    }
    /// <summary>
    /// Fulfills the <see cref="IItem.OnPickUp"/> contract.
    /// However, unlike standard items, enemies cannot be placed into the player's backpack.
    /// Instead, attempting to walk into an enemy treats them as an obstacle 
    /// and updates the log to prompt the player for combat.
    /// </summary>
    /// <param name="state">The current global state of the game, used to update the UI log.</param>
}
