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
        state.Log = $"Attack direction? [{Keybinds.MoveUp}{Keybinds.MoveDown}{Keybinds.MoveLeft}{Keybinds.MoveRight}] or ESC.";

        state.IsWaitingForSecondaryInput = true;
        state.PendingAction = (dirKey) => ProcessDirectionInput(state, dirKey);
    }

    private void ProcessDirectionInput(GameState state, ConsoleKey dirKey)
    {
        int targetX = state.Player.X, targetY = state.Player.Y;

        if (DirectionMap.TryGetValue(dirKey, out var offset))
        {
            targetX += offset.dx;
            targetY += offset.dy;
        }
        else
        {
            state.Player.LogMessage = "Invalid direction.";
            state.IsWaitingForSecondaryInput = false;
            return;
        }

        var enemy = state.Map.GetEnemyAt(targetX, targetY);
        if (enemy == null)
        {
            state.Player.LogMessage = "You swing at the empty air.";
            state.IsWaitingForSecondaryInput = false;
            state.PendingAction = null;
            state.Log = "";
            return;
        }

        state.Log = $"Fighting {enemy.Name} (HP: {enemy.Health}). Choose attack: [1] Normal, [2] Stealth, [3] Magic.";

        state.PendingAction = (attackKey) => ProcessAttackTypeInput(state, attackKey, enemy);
    }

    private void ProcessAttackTypeInput(GameState state, ConsoleKey attackKey, Enemy enemy)
    {
        state.IsWaitingForSecondaryInput = false;
        state.PendingAction = null;
        state.Log = "";

        var attackFactories = new Dictionary<ConsoleKey, Func<Player, AttackVisitor>>
    {
        { ConsoleKey.D1, p => new NormalAttack(p) },
        { ConsoleKey.D2, p => new StealthAttack(p) },
        { ConsoleKey.D3, p => new MagicAttack(p) }
    };

        if (!attackFactories.TryGetValue(attackKey, out var createAttack))
        {
            GameLogger.Instance.Log(LogType.System, "Attack cancelled.");
            return;
        }

        AttackVisitor visitor = createAttack(state.Player);
        IInventoryItem? activeWeapon = state.Player.RightHand ?? state.Player.LeftHand;

        if (activeWeapon != null)
            activeWeapon.Accept(visitor);
        else visitor.VisitNonWeapon();

        int playerDamageDone = Math.Max(0, visitor.CalculatedDamage - enemy.Armor);

        enemy.TakeDamage(playerDamageDone);

        GameLogger.Instance.Log(LogType.Combat, $"{state.Player.Name} dealt {playerDamageDone} dmg to {enemy.Name}.");

        if (enemy.IsDead)
        {
            state.Map.Enemies.Remove(enemy);
            state.Player.LogMessage = $"You killed {enemy.Name} with {playerDamageDone} dmg!";
            GameLogger.Instance.Log(LogType.Combat, $"{enemy.Name} died in combat.");
            return;
        }

        int damageTakenByPlayer = Math.Max(0, enemy.AttackDamage - visitor.CalculatedDefense);
        state.Player.TakeDamage(damageTakenByPlayer);

        state.Player.LogMessage = $"Hit for {playerDamageDone}. {enemy.Name} hits back for {damageTakenByPlayer}! (HP: {state.Player.Health})";
        GameLogger.Instance.Log(LogType.Combat, $"{state.Player.Name} took {damageTakenByPlayer} dmg from {enemy.Name}.");

        if (state.Player.IsDead)
        {
            state.IsGameOver = true;
            GameLogger.Instance.Log(LogType.System, $"{state.Player.Name} died in combat.");
        }
    }
}