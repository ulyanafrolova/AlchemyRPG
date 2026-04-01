namespace AlchemyRPG;

/// <summary>
/// Implements the <see cref="IItem"/> interface to allow integration with the 
/// existing map grid system, meaning enemies occupy tiles just like items, 
/// but trigger combat interactions instead of being added to the inventory.
/// </summary>
public class Enemy : IItem
{
    /// <summary>
    /// Gets the display name of the enemy (e.g., "Goblin", "Orc").
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the symbol used to render this enemy on the game map.
    /// </summary>
    public char Symbol => Tiles.Enemy;
    /// <summary>
    /// Gets or sets the enemy's current health points.
    /// </summary>
    public int Health { get; set; }
    /// <summary>
    /// Gets the base attack power of the enemy. 
    /// </summary>
    public int AttackDamage { get; }
    /// <summary>
    /// Gets the defensive armor rating of the enemy.
    /// </summary>
    public int Armor { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Enemy"/> class with specified combat statistics.
    /// </summary>
    /// <param name="name">The name of the enemy species or type.</param>
    /// <param name="health">The starting health points.</param>
    /// <param name="attackDamage">The fixed amount of base damage the enemy deals.</param>
    /// <param name="armor">The flat damage reduction applied to incoming attacks.</param>
    public Enemy(string name, int health, int attackDamage, int armor)
    {
        Name = name;
        Health = health;
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
    public void OnPickUp(GameState state)
    {
        state.Player.LogMessage = $"{Name} blocks your path! Press [{Keybinds.Attack}] to fight.";
    }
}
