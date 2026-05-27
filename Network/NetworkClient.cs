using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;

namespace AlchemyRPG;

/// <summary>
/// Manages the client-side TCP connection to the authoritative game server.
/// Handles asynchronous transmission of commands and the continuous reception of game state updates.
/// </summary>
public class NetworkClient
{
    private volatile bool _isRunning = true;
    private readonly CancellationTokenSource _cts = new();
    private readonly string _ip;
    private readonly int _port;
    
    // Callbacks to bridge the network layer with the local state container and view
    private readonly Action<GameStateDTO> _onStateReceived;
    private readonly Action<string, string> _onError;
    private readonly Action<InitialDataDTO> _onInitReceived;

    /// <summary>
    /// A thread-safe, unbounded channel acting as a queue for outbound command messages.
    /// Ensures that UI interactions do not block waiting for network I/O.
    /// </summary>
    private readonly Channel<string> _sendChannel = 
        Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleWriter = true });

    public NetworkClient(string ip, int port, Action<InitialDataDTO> onInitReceived, Action<GameStateDTO> onStateReceived, Action<string, string> onError)
    {
        _ip = ip;
        _port = port;
        _onInitReceived = onInitReceived;
        _onStateReceived = onStateReceived;
        _onError = onError;
    }

    /// <summary>
    /// Serializes a command DTO and places it into the asynchronous send channel.
    /// </summary>
    /// <param name="commandDto">The command data transfer object to send.</param>
    public void SendCommand(NetworkCommandDTO commandDto)
    {
        if (!_isRunning) return;
        string json = JsonSerializer.Serialize(commandDto);
        _sendChannel.Writer.TryWrite(json);
    }

    /// <summary>
    /// Starts the asynchronous background tasks for reading from and writing to the TCP stream.
    /// </summary>
    public void Start()
    {
        Task.Run(async () =>
        {
            try
            {
                using TcpClient client = new TcpClient(_ip, _port);
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream);
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                // Launch the background sender task within the same scope to safely utilize the StreamWriter
                var senderTask = Task.Run(async () =>
                {
                    await foreach (var json in _sendChannel.Reader.ReadAllAsync(_cts.Token))
                    {
                        try { await writer.WriteLineAsync(json); }
                        catch { break; }
                    }
                });

                // The first message from the server is always the static initialization data
                string? firstLine = await reader.ReadLineAsync(_cts.Token);
                if (!string.IsNullOrWhiteSpace(firstLine))
                {
                    var initData = JsonSerializer.Deserialize<InitialDataDTO>(firstLine);
                    if (initData != null) _onInitReceived(initData);
                }

                // Continuously listen for dynamic game state updates
                while (_isRunning)
                {
                    string? jsonLine = await reader.ReadLineAsync(_cts.Token);
                    if (jsonLine == null) break;
                    if (string.IsNullOrWhiteSpace(jsonLine)) continue;

                    var stateDto = JsonSerializer.Deserialize<GameStateDTO>(jsonLine);
                    if (stateDto != null)
                        _onStateReceived(stateDto);
                }

                _sendChannel.Writer.TryComplete();
                await senderTask;
            }
            catch (OperationCanceledException) { /* Expected during graceful shutdown */ }
            catch (Exception ex)
            {
                _isRunning = false;
                _onError("Client Error", ex.Message);
            }
        });
    }

    /// <summary>
    /// Initiates a graceful shutdown of the client connection and background tasks.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _cts.Cancel();
        _sendChannel.Writer.TryComplete();
    }
}