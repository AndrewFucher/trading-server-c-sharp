using Microsoft.Extensions.Logging;
using TradingServer.Client.WebSocketWrapper;

namespace TradingServer.Client.Binance.WebSocket;

public class BinanceWebSocketWrapper : NativeWebSocketClientClientWrapper
{
    private const int IntervalBetweenMessagesToSendMs = 300;
    private const string BinanceWebsocketUrl = "wss://stream.binance.com:9443/ws";
    private Func<WebSocketMessage, Task> OnMessage;
    
    public BinanceWebSocketWrapper(
        ILogger logger,
        Func<WebSocketMessage, Task> onMessage,
        int pingIntervalMs = 180000)
        : base(
            logger,
            BinanceWebsocketUrl,
            IntervalBetweenMessagesToSendMs,
            pingIntervalMs
        )
    {
        OnMessage = onMessage;
    }

    protected override async Task HandleWebSocketMessage(string message)
    {
        await OnMessage.Invoke(new WebSocketMessage() {Message = message});
    }
}