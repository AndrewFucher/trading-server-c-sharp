using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace TradingServer.Common.Serilog;

public static class SerilogExtension
{
    public static IServiceCollection AddSerilog(this IServiceCollection serviceCollection)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        
        return serviceCollection.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(logger));;
    }
}