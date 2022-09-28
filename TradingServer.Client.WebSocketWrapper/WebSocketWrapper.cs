using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TradingServer.Client.WebSocketWrapper.Exceptions;
using Websocket.Client;

namespace TradingServer.Client.WebSocketWrapper;

public class WebSocketWrapper
{
    private readonly ILogger _logger;
    protected readonly IWebsocketClient WebsocketClient;
    protected CancellationToken CancellationToken = new();
    protected readonly ConcurrentQueue<string> MessagesToSend = new();
    protected int IntervalBetweenMessagesToSendMilliseconds { get; set; }

    public event EventHandler<WebSocketMessage> OnMessageEventHandler;

    public WebSocketWrapper(
        ILogger logger,
        string url,
        int intervalBetweenMessagesToSendMilliseconds = 1000,
        EventHandler<WebSocketMessage>? onMessage = null,
        int pingIntervalMinutes = 3
    )
    {
        _logger = logger;
        var webSocketFactory = new Func<ClientWebSocket>(() => new ClientWebSocket()
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromMinutes(pingIntervalMinutes)
            }
        });
        WebsocketClient = new WebsocketClient(new Uri(url), webSocketFactory);

        IntervalBetweenMessagesToSendMilliseconds = intervalBetweenMessagesToSendMilliseconds;

        if (onMessage != null)
            OnMessageEventHandler = onMessage;
    }

    public virtual void Send(object message)
    {
        Send(JsonConvert.SerializeObject(message, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore }));
    }
    
    public virtual void Send(string message)
    {
        MessagesToSend.Enqueue(message);
    }

    public virtual async Task ConnectAsync()
    {
        if (!WebsocketClient.IsRunning)
            await WebsocketClient.Start();
        
        if (!WebsocketClient.IsRunning)
            throw new FailedWebsocketStartException();
    }

    public virtual async Task StartAsync()
    {
        if (!WebsocketClient.IsRunning)
            await ConnectAsync();
        
        ConfigureListener();

        // Start running async sender
        await Task.Factory.StartNew(SendMessageFromQueue, TaskCreationOptions.LongRunning);
    }

    protected virtual async Task SendMessageFromQueue()
    {
        while (true)
        {
            if (MessagesToSend.IsEmpty)
            {
                await Task.Delay(5 * 1000, CancellationToken);
                continue;
            }

            if (MessagesToSend.TryDequeue(out var message))
                await WebsocketClient.SendInstant(message);

            await Task.Delay(IntervalBetweenMessagesToSendMilliseconds, CancellationToken);
        }
    }

    protected virtual void ConfigureListener()
    {
        if (OnMessageEventHandler == null)
            throw new ArgumentException("At least one subscriber must be declared");
        
        WebsocketClient
            .MessageReceived
            .Subscribe(message =>
                OnMessageEventHandler.Invoke(this, new WebSocketMessage {Message = message.Text})
            );
    }

    public void Dispose()
    {
        WebsocketClient.Dispose();
    }
}