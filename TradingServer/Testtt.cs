using System.Diagnostics;
using TradingServer.Client.Binance.Abstractions;
using TradingServer.Client.Binance.Dtos;
using TradingServer.Client.Binance.Enums;

namespace TradingServer;

public class Testtt : IHostedService
{
    private readonly ILogger<Testtt> _logger;
    private readonly IBinanceSpotWebSocketManager _binanceSpotWebSocketManager;

    public Testtt(ILogger<Testtt> logger, IBinanceSpotWebSocketManager binanceSpotWebSocketManager)
    {
        _logger = logger;
        _binanceSpotWebSocketManager = binanceSpotWebSocketManager;
        var a = PrintEventMiniTicker;
        _binanceSpotWebSocketManager.AddCallback(EventType.MiniTicker24Hour, PrintEventMiniTicker);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _binanceSpotWebSocketManager.StartAsync();
        var subscriptions =
            await _binanceSpotWebSocketManager.SubscribeToMiniTicker24Hour("ethusdt", "btcusdt", "adausdt",
                "VIBBUSD".ToLower(), "ltcbtc", "bnbbtc", "noebtc", "qtumeth", "eoseth", "gasbtc", "wtcbtc", "lrcbtc",
                "qtubtc", "omgbtc", "zrxbtc", "kncbtc", "snmbtc", "iotabtc");
        await _binanceSpotWebSocketManager.SubscribeToAllMiniTicker24Hour();
        _logger.LogInformation($"Current subscriptions for Mini Ticker 24h {string.Join(",", subscriptions)}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopped logging");
        return Task.CompletedTask;
    }

    private Task PrintEventMiniTicker(WebSocketEvent @event)
    {
        var fullEvent = (MiniTickerWebSocketEvent) @event;
        // GC.Collect();
        // _logger.LogInformation($"{_process.PrivateMemorySize64}");

        // _logger.LogInformation($"DateTime: {DateTimeOffset.FromUnixTimeMilliseconds(fullEvent.EventTime).DateTime} " +
        //                        $"Symbol: {fullEvent.Symbol} " +
        //                        $"Last price: {fullEvent.ClosePrice} ");
        return Task.CompletedTask;
    }
}