using Newtonsoft.Json;

namespace TradingServer.Client.Binance.Dtos;

public class AggTradeWebSocketEvent : WebSocketEvent
{
    [JsonProperty(PropertyName = "a")]
    public int AggTradeId { get; set; }
    
    [JsonProperty(PropertyName = "p")]
    public decimal Price { get; set; }
    
    [JsonProperty(PropertyName = "q")]
    public decimal Quantity { get; set; }
    
    [JsonProperty(PropertyName = "f")]
    public int FirstTradeId { get; set; }
    
    [JsonProperty(PropertyName = "l")]
    public int LastTradeId { get; set; }
    
    [JsonProperty(PropertyName = "T")]
    public long TradeTime { get; set; }
    
    [JsonProperty(PropertyName = "m")]
    public int IsBuyerMarketMaker { get; set; }
}