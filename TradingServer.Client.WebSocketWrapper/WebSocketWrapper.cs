using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TradingServer.Client.WebSocketWrapper.Exceptions;
using Websocket.Client;

namespace TradingServer.Client.WebSocketWrapper;

public class WebSocketWrapper : IDisposable
{
    private readonly ILogger _logger;
    protected readonly IWebsocketClient WebsocketClient;
    protected CancellationToken CancellationToken = new();
    protected readonly ConcurrentQueue<string> MessagesToSend = new();
    protected int IntervalBetweenMessagesToSendMilliseconds { get; set; }

    public event EventHandler<MessageEvent> OnMessageEventHandler;

    public WebSocketWrapper(
        ILogger logger,
        string url,
        int intervalBetweenMessagesToSendMilliseconds = 1000,
        EventHandler<MessageEvent>? onMessage = null,
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
        Task.Run(async () => await SendMessageFromQueue());
    }

    protected virtual async Task SendMessageFromQueue()
    {
        if (MessagesToSend.IsEmpty)
            Task.Run(async () =>
            {
                await Task.Delay(5 * 1000, CancellationToken);
                await SendMessageFromQueue();
            });

        if (MessagesToSend.TryDequeue(out var message))
            await WebsocketClient.SendInstant(message);

        Task.Run(async () =>
        {
            await Task.Delay(IntervalBetweenMessagesToSendMilliseconds, CancellationToken);
            await SendMessageFromQueue();
        });
    }

    protected virtual void ConfigureListener()
    {
        if (OnMessageEventHandler == null)
            throw new ArgumentException("At least one subscriber must be declared");
        
        WebsocketClient
            .MessageReceived
            .Subscribe(message =>
                OnMessageEventHandler.Invoke(this, new MessageEvent {Message = message.Text})
            );
    }

    public void Dispose()
    {
        WebsocketClient.Dispose();
    }
}