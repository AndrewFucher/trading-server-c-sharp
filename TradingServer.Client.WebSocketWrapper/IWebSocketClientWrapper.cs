namespace TradingServer.Client.WebSocketWrapper;

public interface IWebSocketClientWrapper : IDisposable
{
    bool IsRunning { get; }
    
    Task StartAsync();
    Task StopAsync();
    
    Task SendAsync(object message);
    Task SendAsync(string message);
}