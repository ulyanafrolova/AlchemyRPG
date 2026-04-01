using System;

namespace AlchemyRPG;

/// <summary>
/// Represents the main game engine. 
/// Handles the initialization of the game world, manages the main game loop.
/// </summary>
public class Game
{
    /// <summary>
    /// Global state of the game: the map, player, and logs.
    /// </summary>
    private readonly GameState _state;

    /// <summary>
    /// Flag indicating whether the main game loop should continue running.
    /// </summary>
    private bool _isRunning = true;

    /// <summary>
    /// The input handler responsible for mapping keyboard keys to specific commands.
    /// </summary>
    private readonly InputHandler _inputHandler;


    /// <summary>
    /// Initializes a new instance of the Game class.
    /// Sets up the console, constructs the dungeon using the Builder pattern, 
    /// and prepares the initial game state.
    /// </summary>
    public Game()
    {
        // Support special characters ('█' or '¶')
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;

        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(Config.WindowWidth, Config.WindowHeight);
                Console.SetBufferSize(Config.WindowWidth, Config.WindowHeight);
            }
            catch { } 
        }

        // Initialize the command pattern invoker
        _inputHandler = new InputHandler();

        // Use the Builder pattern to generate the dungeon
        var builder = new DungeonBuilder();
        var director = new DungeonDirector(builder);

        // Construct a standard dungeon with rooms, corridors, weapons, and junk
        director.ConstructStandardDungeon();

        _state = new GameState
        {
            Player = new Player(1, 1),
            Map = builder.GetMap(),
            Instructions = builder.GetInstructions(),
            TutorialText = builder.GetTutorialText()
        };
    }

    /// <summary>
    /// Starts the main game loop. Continuously clears the screen, draws the current state, 
    /// and waits for player input until the game is exited.
    /// </summary>
    public void Run()
    {
        Console.Clear();

        new HelpCommand().Execute(_state);

        while (_isRunning && !_state.IsGameOver)
        {
            // Reset cursor to top-left to redraw the frame smoothly without flickering
            Console.SetCursorPosition(0, 0);

            // 1. Render the game world
            _state.Map.Draw(_state);

            // 2. Wait for the player to press a key
            var keyInfo = Console.ReadKey(true);

            // 3. Delegate the key press to the InputHandler.
            // If the handler returns false (e.g., Escape was pressed), the loop will end.
            _isRunning = _inputHandler.HandleInput(keyInfo.Key, _state);
        }
        if (_state.IsGameOver)
        {
            Console.Clear();
            Console.WriteLine("\n\n-----------------------------------------");
            Console.WriteLine("               GAME OVER                 ");
            Console.WriteLine("       You have lost in the battle!      ");
            Console.WriteLine("-----------------------------------------\n\n");
        }
    }
}