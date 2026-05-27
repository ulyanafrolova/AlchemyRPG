using System;
using System.Collections.Generic;

namespace AlchemyRPG;

/// <summary>
/// A factory class responsible for creating specific instances of <see cref="AttackVisitor"/> 
/// based on the requested <see cref="AttackType"/>. 
/// Utilizes the Factory Method pattern combined with a dynamic registry to avoid hardcoded switch statements.
/// </summary>
public static class AttackFactory
{
    /// <summary>
    /// A centralized registry that maps each <see cref="AttackType"/> to a factory delegate 
    /// capable of instantiating the corresponding <see cref="AttackVisitor"/>.
    /// </summary>
    private static readonly Dictionary<AttackType, Func<Player, AttackVisitor>> _registry = new()
    {
        { AttackType.Normal, p => new NormalAttack(p) },
        { AttackType.Stealth, p => new StealthAttack(p) },
        { AttackType.Magic, p => new MagicAttack(p) }
    };

    /// <summary>
    /// Dynamically registers a new attack type and its corresponding factory method into the registry.
    /// This allows the combat system to be extended with new attack types at runtime without modifying this class.
    /// </summary>
    /// <param name="type">The unique enum identifier for the new attack type.</param>
    /// <param name="factoryMethod">A delegate that takes a <see cref="Player"/> context and returns the specific <see cref="AttackVisitor"/>.</param>
    public static void RegisterAttackType(AttackType type, Func<Player, AttackVisitor> factoryMethod)
    {
        _registry[type] = factoryMethod;
    }

    /// <summary>
    /// Creates and returns an appropriate <see cref="AttackVisitor"/> for the specified attack type.
    /// </summary>
    /// <param name="type">The requested type of the attack.</param>
    /// <param name="player">The player initiating the attack, whose stats will be injected into the visitor.</param>
    /// <returns>An instance of a class derived from <see cref="AttackVisitor"/>. Defaults to <see cref="NormalAttack"/> if the requested type is unknown.</returns>
    public static AttackVisitor Create(AttackType type, Player player)
    {
        // Attempt to retrieve and invoke the specific factory method from the registry
        if (_registry.TryGetValue(type, out var factory))
        {
            return factory(player);
        }
        
        // Fallback to a safe default attack if the provided type is not registered
        return new NormalAttack(player); 
    }
} 