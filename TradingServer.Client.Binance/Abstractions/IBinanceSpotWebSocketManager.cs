using TradingServer.Client.Binance.Dtos;

namespace TradingServer.Client.Binance.Abstractions;

public interface IBinanceSpotWebSocketManager
{
    Task StartAsync();
    Task<WebSocketResponse> SendRequest(WebSocketRequest request);
    
    /// <summary>
    /// Subscribes to binance websocket 24 hour mini ticker.
    /// </summary>
    /// <param name="symbols">Symbol or list of symbols to subscribe to.</param>
    /// <returns>List of subscriptions only to 24 hour mini ticker.</returns>
    Task<IEnumerable<string>> SubscribeToMiniTicker24Hour(params string[] symbols);

    /// <summary>
    /// Subscribes to binance websocket 24 hour mini ticker.
    /// </summary>
    /// <param name="symbols">List of symbols to subscribe to.</param>
    /// <returns>List of subscriptions only to 24 hour mini ticker.</returns>
    Task<IEnumerable<string>> SubscribeToMiniTicker24Hour(IEnumerable<string> symbols);

    /// <summary>
    /// Subscribes to binance websocket 24 hour mini ticker all symbols.
    /// </summary>
    Task SubscribeToAllMiniTicker24Hour();

    /// <summary>
    /// Returns list of subscriptions.
    /// </summary>
    /// <returns>List of subscriptions.</returns>
    Task<IEnumerable<string>> GetSubscriptions();
}