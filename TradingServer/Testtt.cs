namespace TradingServer;

public class Testtt : IHostedService
{
    private readonly ILogger<Testtt> _logger;
    public Testtt(ILogger<Testtt> logger)
    {
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started logging");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopped logging");
        return Task.CompletedTask;
    }
}