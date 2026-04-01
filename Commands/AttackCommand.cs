namespace AlchemyRPG;

/// <summary>
/// Manages the combat interaction between the player and an enemy.
/// This command pauses the game loop to ask for the attack direction and type, 
/// and then uses the Visitor pattern to calculate damage without checking weapon types.
/// </summary>
public class AttackCommand : ICommand
{
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
        if (dirKey == ConsoleKey.Escape) { state.Player.LogMessage = "Attack cancelled."; return; }
        // Determine the target coordinates based on the chosen direction
        int targetX = state.Player.X, targetY = state.Player.Y;
        if (dirKey == Keybinds.MoveUp) targetY--;
        else if (dirKey == Keybinds.MoveDown) targetY++;
        else if (dirKey == Keybinds.MoveLeft) targetX--;
        else if (dirKey == Keybinds.MoveRight) targetX++;
        else { state.Player.LogMessage = "Invalid direction."; return; }
        // 2. Check if there is an enemy in the target cell
        var itemsInTargetCell = state.Map.GetItemsAt(targetX, targetY);
        var enemy = itemsInTargetCell.OfType<Enemy>().FirstOrDefault();
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
        AttackVisitor visitor;
        // Select the correct Visitor
        if (attackKey == ConsoleKey.D1) visitor = new NormalAttack(state.Player);
        else if (attackKey == ConsoleKey.D2) visitor = new StealthAttack(state.Player);
        else if (attackKey == ConsoleKey.D3) visitor = new MagicAttack(state.Player);
        else { state.Player.LogMessage = "Attack cancelled."; return; }
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
        // 6. Check for enemy death
        if (enemy.Health <= 0)
        {
            state.Map.RemoveItem(targetX, targetY, enemy);
            state.Player.LogMessage = $"You hit {enemy.Name} for {playerDamageDone} dmg. It dies!";
            return;
        }
        // 7. Calculate damage taken by the player
        int damageTakenByPlayer = Math.Max(0, enemy.AttackDamage - visitor.CalculatedDefense);
        state.Player.Health -= damageTakenByPlayer;

        state.Player.LogMessage = $"You hit for {playerDamageDone} dmg. {enemy.Name} hits back for {damageTakenByPlayer} dmg! (HP left: {state.Player.Health})";
        // 8. Check for player death
        if (state.Player.Health <= 0)
        {
            state.IsGameOver = true;
        }
    }
}