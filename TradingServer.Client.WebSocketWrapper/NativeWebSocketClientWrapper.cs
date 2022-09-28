using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TradingServer.Client.WebSocketWrapper.Exceptions;
using Websocket.Client;

namespace TradingServer.Client.WebSocketWrapper;

public abstract class NativeWebSocketClientClientWrapper : IWebSocketClientWrapper
{
    public bool IsRunning { get; protected set; }

    private Task _listenerExecutionTask;
    private Task _sendingMessagesExecutionTask;

    private ClientWebSocket _clientWebSocket;
    private readonly string _url;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly int _bufferSize;
    private readonly ConcurrentQueue<string> _messagesToSend = new();

    private readonly int _reconnectionDelayMs;
    private readonly int _maxReconnectionCount;

    private readonly int _messageSendingIntervalMs;

    private int _reconnectionCount = 0;

    public NativeWebSocketClientClientWrapper(ILogger logger, string url, int messageSendingIntervalMs = 300,
        int pingIntervalMs = 180000,
        int maxReconnectionCount = 3,
        int reconnectionDelayMs = 10000, int bufferSize = 16384)
    {
        _logger = logger;
        _url = url;
        _bufferSize = bufferSize;
        _messageSendingIntervalMs = messageSendingIntervalMs;
        _reconnectionDelayMs = reconnectionDelayMs;
        _maxReconnectionCount = maxReconnectionCount;

        _clientWebSocket = new ClientWebSocket
            {Options = {KeepAliveInterval = TimeSpan.FromMilliseconds(pingIntervalMs)}};
    }

    protected abstract void HandleWebSocketMessage(string message);

    public virtual async Task StartAsync()
    {
        if (!await TryConnect())
            throw new FailedWebsocketStartException();
        _listenerExecutionTask = ListenWebSocket();
        _sendingMessagesExecutionTask = SendMessagesToWebSocket();
    }

    public virtual async Task StopAsync()
    {
        if (_listenerExecutionTask == null)
            return;
        if (_sendingMessagesExecutionTask == null)
            return;

        try
        {
            _stoppingCts.Cancel();
        }
        finally
        {
            await Task.WhenAll(_listenerExecutionTask, _sendingMessagesExecutionTask);
        }
    }

    public virtual Task SendAsync(object message)
    {
        return SendAsync(JsonConvert.SerializeObject(message,
            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}));
    }

    public virtual Task SendAsync(string message)
    {
        _messagesToSend.Enqueue(message);
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        _clientWebSocket.Dispose();
    }

    protected virtual async Task SendMessagesToWebSocket()
    {
        try
        {
            while (!_stoppingCts.Token.IsCancellationRequested)
            {
                if (_messagesToSend.TryDequeue(out var message))
                {
                    try
                    {
                        _logger.LogInformation($"Sending message {message}");
                        await _clientWebSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text,
                            true, _stoppingCts.Token);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Failed to send message {message}. Error: {e.Message}");
                    }
                }

                await Task.Delay(3000);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected virtual async Task ListenWebSocket()
    {
        while (!_stoppingCts.Token.IsCancellationRequested)
        {
            try
            {
                WebSocketReceiveResult receiveResult;
                var buffer = WebSocket.CreateClientBuffer(_bufferSize, _bufferSize);
                string message;
                while (_clientWebSocket.State != WebSocketState.Closed)
                {
                    await using (var ms = new MemoryStream())
                    {
                        do
                        {
                            receiveResult = await _clientWebSocket.ReceiveAsync(buffer, _stoppingCts.Token);
                            if (!buffer.Any() || buffer.Array == null)
                                return;
                            await ms.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count);
                        } while (!receiveResult.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);
                        
                        message = Encoding.UTF8.GetString(ms.ToArray());
                    }

                    if (_clientWebSocket.State is WebSocketState.CloseReceived or WebSocketState.Closed)
                        break;

                    if (_clientWebSocket.State == WebSocketState.Open &&
                        receiveResult.MessageType != WebSocketMessageType.Close)
                    {

                        HandleWebSocketMessage(message);
                    }
                }

                await Task.Delay(_reconnectionDelayMs);
                await TryConnect();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in listener {e.Message}");
            }
        }

        _clientWebSocket.Dispose();
        _clientWebSocket = null!;
    }

    protected virtual async Task<bool> TryConnect()
    {
        _logger.LogInformation($"Connecting to server {_url}");
        _reconnectionCount = 1;

        while (_reconnectionCount < _maxReconnectionCount + 1)
        {
            try
            {
                await _clientWebSocket.ConnectAsync(new Uri(_url), _stoppingCts.Token);
                break;
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot connect to websocket {e.Message}. Attempt #[{_reconnectionCount}]");
            }

            await Task.Delay(_reconnectionDelayMs);
            _reconnectionCount++;
        }

        if (_reconnectionCount != _maxReconnectionCount)
        {
            IsRunning = true;
            return true;
        }

        IsRunning = false;
        return false;
    }
}