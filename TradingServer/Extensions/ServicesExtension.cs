using TradingServer.Client.Binance.Abstractions;
using TradingServer.Client.Binance.WebSocket;

namespace TradingServer.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBinanceSpotWebSocketManager>(
            s => new BinanceSpotWebSocketManager(s.GetRequiredService<ILogger<BinanceSpotWebSocketManager>>())
        );
        serviceCollection.AddHostedService<Testtt>();
        return serviceCollection;
    }
}