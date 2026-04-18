namespace AlchemyRPG;

/// <summary>
/// Manages the combat interaction between the player and an enemy.
/// This command pauses the game loop to ask for the attack direction and type, 
/// and then uses the Visitor pattern to calculate damage without checking weapon types.
/// </summary>
public class AttackCommand : ICommand
{
    private static readonly Dictionary<ConsoleKey, (int dx, int dy)> DirectionMap = new()
    {
        { Keybinds.MoveUp, (0, -1) },
        { Keybinds.MoveDown, (0, 1) },
        { Keybinds.MoveLeft, (-1, 0) },
        { Keybinds.MoveRight, (1, 0) }
    };
    /// <summary>
    /// The player can always attempt to initiate an attack.
    /// </summary>
    public bool CanExecute(GameState state) => true;

    public void Execute(GameState state)
    {
        // 1. Ask the player for the attack direction
        state.Log = $"Attack direction? [{Keybinds.MoveUp}{Keybinds.MoveDown}{Keybinds.MoveLeft}{Keybinds.MoveRight}] or ESC.";
        Console.SetCursorPosition(0, 0);
        state.Map.Draw(state);
        var dirKey = Console.ReadKey(true).Key;
        if (dirKey == ConsoleKey.Escape)
        {
            GameLogger.Instance.Log(LogType.System, "Attack cancelled.");
            return;
        }
        // Determine the target coordinates based on the chosen direction
        int targetX = state.Player.X, targetY = state.Player.Y;
        if (DirectionMap.TryGetValue(dirKey, out var offset))
        {
            targetX += offset.dx;
            targetY += offset.dy;
        }
        else
        {
            state.Player.LogMessage = "Invalid direction.";
            return;
        }
        // 2. Check if there is an enemy in the target cell
        var enemy = state.Map.GetEnemyAt(targetX, targetY);

        if (enemy == null)
        {
            state.Player.LogMessage = "You swing at the empty air.";
            return;
        }
        // 3. The player selects the attack type
        state.Log = $"Fighting {enemy.Name} (HP: {enemy.Health}). Choose attack: [1] Normal, [2] Stealth, [3] Magic.";
        Console.SetCursorPosition(0, 0);
        state.Map.Draw(state);

        var attackKey = Console.ReadKey(true).Key;
        var attackFactories = new Dictionary<ConsoleKey, Func<Player, AttackVisitor>>
        {
            { ConsoleKey.D1, p => new NormalAttack(p) },
            { ConsoleKey.D2, p => new StealthAttack(p) },
            { ConsoleKey.D3, p => new MagicAttack(p) }
        };

        // Attempt to get the correct factory function based on the key pressed
        if (!attackFactories.TryGetValue(attackKey, out var createAttack))
        {
            GameLogger.Instance.Log(LogType.System, "Attack cancelled.");
            return;
        }

        // Execute the factory function to create the specific Visitor
        AttackVisitor visitor = createAttack(state.Player);

        // Get the weapon currently held by the player
        IInventoryItem? activeWeapon = state.Player.RightHand ?? state.Player.LeftHand;
        // 4. Visitor pattern: the attack (visitor) is passed to the weapon, which will
        // route it to the correct formula based on its type
        if (activeWeapon != null)
        {
            activeWeapon.Accept(visitor, activeWeapon);
        }
        else
        {
            visitor.VisitNonWeapon();
        }
        // 5. Calculate damage done to the enemy 
        int playerDamageDone = Math.Max(0, visitor.CalculatedDamage - enemy.Armor);
        enemy.Health -= playerDamageDone;

        GameLogger.Instance.Log(LogType.Combat,$"{state.Player.Name} dealt {playerDamageDone} dmg to {enemy.Name} using {visitor.GetType().Name}.");

        // 6. Check for enemy death
        if (enemy.Health <= 0)
        {
            state.Map.Enemies.Remove(enemy);
            state.Player.LogMessage = $"You hit {enemy.Name} for {playerDamageDone} dmg. It dies!";
            return;
        }
        // 7. Calculate damage taken by the player
        int damageTakenByPlayer = Math.Max(0, enemy.AttackDamage - visitor.CalculatedDefense);
        state.Player.Health -= damageTakenByPlayer;

        state.Player.LogMessage = ($"You hit for {playerDamageDone} dmg. {enemy.Name} hits back for {damageTakenByPlayer} dmg! (HP left: {state.Player.Health})");
        // 8. Check for player death
        if (state.Player.Health <= 0)
        {
            state.IsGameOver = true;
            GameLogger.Instance.Log(LogType.System, $"{state.Player.Name} died in combat.");
        }
    }
}