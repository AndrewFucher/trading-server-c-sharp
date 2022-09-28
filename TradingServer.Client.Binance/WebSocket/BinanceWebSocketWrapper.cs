using Microsoft.Extensions.Logging;
using TradingServer.Client.WebSocketWrapper;

namespace TradingServer.Client.Binance.WebSocket;

public class BinanceWebSocketWrapper : NativeWebSocketClientClientWrapper
{
    private const int IntervalBetweenMessagesToSendMs = 300;
    private const string BinanceWebsocketUrl = "wss://stream.binance.com:9443/ws";
    private event EventHandler<WebSocketMessage> OnMessage;
    
    public BinanceWebSocketWrapper(
        ILogger logger,
        EventHandler<WebSocketMessage> onMessage,
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

    protected override void HandleWebSocketMessage(string message)
    {
        OnMessage.Invoke(this, new WebSocketMessage() {Message = message});
    }
}