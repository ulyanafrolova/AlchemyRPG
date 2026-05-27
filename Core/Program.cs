using System;
using System.Collections.Generic;
using System.Threading;

namespace AlchemyRPG;

/// <summary>
/// The main entry point for the Alchemy RPG application.
/// Handles startup configuration and routes the application into either Server or Client mode.
/// </summary>
class Program
{
    /// <summary>
    /// The primary execution method. Parses command-line arguments to determine the run mode,
    /// or prompts the user interactively if no arguments are provided.
    /// </summary>
    /// <param name="args">Command-line arguments (e.g., --server, --client).</param>
    static void Main(string[] args)
    {
        GameConfig config = GameConfig.Load("config.json");
        string? serverArg = GetArg(args, "--server");
        string? clientArg = GetArg(args, "--client");

        if (serverArg != null)
        {
            int port = int.TryParse(serverArg, out int p) ? p : config.DefaultPort;
            RunServer(port);
            return;
        }

        if (clientArg != null)
        {
            string ip = config.DefaultIp;
            int port = config.DefaultPort;
            if (clientArg.Contains(':'))
            {
                var parts = clientArg.Split(':');
                ip = parts[0];
                port = int.TryParse(parts[1], out int cp) ? cp : config.DefaultPort;
            }
            else if (!string.IsNullOrWhiteSpace(clientArg))
            {
                ip = clientArg;
            }
            RunClient(ip, port);
            return;
        }

        // Interactive selection fallback if no command-line arguments are provided
        Console.Clear();
        Console.WriteLine("=== ALCHEMY RPG MULTIPLAYER ===");
        Console.Write("Start as (S)erver or (C)lient? ");
        var input = Console.ReadLine()?.Trim().ToUpperInvariant();

        if (input == "S")
        {
            RunServer(config.DefaultPort);
        }
        else if (input == "C")
        {
            Console.Write($"Enter Server IP:Port (leave empty for {config.DefaultIp}:{config.DefaultPort}): ");
            string raw = Console.ReadLine() ?? "";
            string ip = config.DefaultIp;
            int port = config.DefaultPort;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                var parts = raw.Split(':');
                ip = parts[0];
                if (parts.Length > 1) int.TryParse(parts[1], out port);
            }
            RunClient(ip, port);
        }
        else
        {
            Console.WriteLine("Unknown option. Exiting.");
        }
    }

    /// <summary>
    /// Helper method to extract specific flag values from the command-line arguments array.
    /// </summary>
    private static string? GetArg(string[] args, string flag)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == flag)
                return i + 1 < args.Length ? args[i + 1] : "";

            if (args[i].StartsWith(flag + "="))
                return args[i][(flag.Length + 1)..];
        }
        return null;
    }

    /// <summary>
    /// Initializes and runs the authoritative, headless game server.
    /// Blocks the main thread to keep the server alive.
    /// </summary>
    /// <param name="port">The TCP port to listen on.</param>
    private static void RunServer(int port)
    {
        Console.Clear();
        Console.WriteLine($"Starting Authoritative Server on port {port}...");

        GameConfig config = GameConfig.Load("config.json");
        ILogger logger = new FileLogger(config.LogDirectory, "Server");
        GameState state = StateFactory.CreateInitialState(config, logger);

        GameEngine engine = new GameEngine(state, logger);
        engine.Start();

        NetworkServer server = new NetworkServer(engine, port, logger);
        server.Start();

        Console.WriteLine("Server is running. Waiting for TCP connections...");
        Console.WriteLine("Press Ctrl+C to terminate.");
        Thread.Sleep(Timeout.Infinite);
    }

    /// <summary>
    /// Initializes the client architecture, connects to the server, and starts the main rendering loop.
    /// </summary>
    /// <param name="ip">The IP address of the server to connect to.</param>
    /// <param name="port">The port of the server to connect to.</param>
    private static void RunClient(string ip, int port)
    {
        Console.WriteLine($"Connecting to {ip}:{port}...");
        var view = new ConsoleView();
        var stateContainer = new ClientStateContainer();

        NetworkClient client = new NetworkClient(
            ip, port,
            initDto =>
            {
                if (!view.IsInitialized)
                    view.Initialize(initDto);
            },
            stateDto =>
            {
                stateContainer.UpdateState(stateDto);
            },
            (errTitle, errMsg) => stateContainer.SetFatalError(errTitle, errMsg)
        );

        client.Start();
        ClientLoop(client, stateContainer, view);
    }

    /// <summary>
    /// The primary execution loop for the client instance.
    /// Handles user input, state rendering, and execution of local UI commands.
    /// </summary>
    static void ClientLoop(NetworkClient client, ClientStateContainer stateContainer, IView view)
    {
        var inputController = new ClientInputController();
        
        // Command registry for global client-side actions
        var globalActions = new Dictionary<ConsoleKey, IClientAction>
        {
            { ConsoleKey.F12, new QuitAction() },
            { ConsoleKey.J, new OpenJournalAction() },
            { ConsoleKey.H, new ShowHelpAction() },
            { Keybinds.Cancel, new ResetInputStateAction() }
        };
        
        bool tutorialShown = false;
        
        while (true)
        {
            var snapshot = stateContainer.GetState();
            
            // Display tutorial screen upon successful initialization
            if (!tutorialShown && view.IsInitialized)
            {
                tutorialShown = true;
                view.RenderFullScreen("DUNGEON INSTRUCTIONS", view.TutorialText);
                Console.Clear();
            }
            
            if (snapshot != null)
            {
                view.Render(snapshot, inputController.GetPrompt());
            }
            
            if (stateContainer.HasError)
            {
                var error = stateContainer.GetError();
                view.RenderFullScreen(error.Title, error.Message);
                client.Stop();
                break;
            }
            
            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                
                // Process global UI commands first
                if (globalActions.TryGetValue(keyInfo.Key, out var action))
                {
                    action.Execute(client, view, snapshot, ref inputController);
                    if (keyInfo.Key == ConsoleKey.F12) break;
                    continue;
                }

                // If not a global command, pass input to the state controller for game actions
                if (snapshot != null)
                {
                    var commandDto = inputController.ProcessInput(keyInfo.Key, snapshot.LocalPlayer);
                    if (commandDto != null)
                        client.SendCommand(commandDto);
                }
            }
            
            Thread.Sleep(30);
        }
    }
}