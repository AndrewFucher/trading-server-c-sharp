using Microsoft.Extensions.Logging;
using TradingServer.Client.WebSocketWrapper;

namespace TradingServer.Client.Binance.WebSocket;

public class BinanceWebSocketWrapper : WebSocketWrapper.WebSocketWrapper
{
    private const int INTERVAL_BETWEEN_MESSEGES_TO_SEND = 1000 / 5 + 300;
    private const string BINANCE_WEBSOCKET_URL = "wss://stream.binance.com:9443/ws";

    public BinanceWebSocketWrapper(
        ILogger logger,
        EventHandler<MessageEvent>? onMessage = null,
        int pingIntervalMinutes = 3)
        : base(
            logger,
            BINANCE_WEBSOCKET_URL,
            INTERVAL_BETWEEN_MESSEGES_TO_SEND,
            onMessage,
            pingIntervalMinutes
        )
    {
    }
}