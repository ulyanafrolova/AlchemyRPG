namespace AlchemyRPG;

/// <summary>
/// Represents the immutable outcome of a single combat exchange between a player and an enemy.
/// Encapsulates the damage statistics and the survival status of both participants.
/// </summary>
public readonly struct AttackResult
{
    /// <summary>
    /// Gets the total amount of damage the player successfully inflicted upon the enemy 
    /// after armor and modifiers were applied.
    /// </summary>
    public int DamageDealt { get; }

    /// <summary>
    /// Gets the total amount of damage the player received from the enemy's counter-attack.
    /// </summary>
    public int DamageTaken { get; }

    /// <summary>
    /// Gets a value indicating whether the enemy's health was reduced to zero or below during this exchange.
    /// </summary>
    public bool IsEnemyDead { get; }

    /// <summary>
    /// Gets a value indicating whether the player's health was reduced to zero or below during this exchange.
    /// </summary>
    public bool IsPlayerDead { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackResult"/> struct.
    /// </summary>
    /// <param name="damageDealt">The calculated damage dealt to the enemy.</param>
    /// <param name="damageTaken">The calculated damage received by the player.</param>
    /// <param name="isEnemyDead">True if the enemy died during the attack; otherwise, false.</param>
    /// <param name="isPlayerDead">True if the player died during the counter-attack; otherwise, false.</param>
    public AttackResult(int damageDealt, int damageTaken, bool isEnemyDead, bool isPlayerDead)
    {
        DamageDealt = damageDealt;
        DamageTaken = damageTaken;
        IsEnemyDead = isEnemyDead;
        IsPlayerDead = isPlayerDead;
    }
}