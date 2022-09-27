namespace TradingServer.Client.Binance.Dtos;

public class WebSocketResponse
{
    public object? Result { get; set; }
    public uint Id { get; set; }

    public bool IsError => Result != null;
}