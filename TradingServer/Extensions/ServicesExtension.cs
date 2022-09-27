namespace TradingServer.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddHostedServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<Testtt>();
    }
}