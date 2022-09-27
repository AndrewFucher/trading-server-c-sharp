using TradingServer.Common.Serilog;
using TradingServer.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((context, configurationBuilder) =>
{
    configurationBuilder.AddEnvironmentVariables();
    configurationBuilder.AddCommandLine(args);
});

builder.ConfigureServices(services =>
{
    services.AddSerilog();
    services.AddServices();
});

var app = builder.Build();

await app.RunAsync();