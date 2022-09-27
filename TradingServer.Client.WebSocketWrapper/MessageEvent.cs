namespace TradingServer.Client.WebSocketWrapper;

public class MessageEvent : EventArgs
{
    public string Message { get; set; }
}