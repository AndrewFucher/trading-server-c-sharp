using Newtonsoft.Json;

namespace TradingServer.Client.Binance.Dtos;

public class MiniTickerWebSocketEvent : WebSocketEvent
{
    [JsonProperty(PropertyName = "c")]
    public decimal ClosePrice { get; set; }
    
    [JsonProperty(PropertyName = "o")]
    public decimal OpenPrice { get; set; }
    
    [JsonProperty(PropertyName = "h")]
    public decimal HighPrice { get; set; }
    
    [JsonProperty(PropertyName = "l")]
    public decimal LowPrice { get; set; }
    
    [JsonProperty(PropertyName = "v")]
    public decimal TotalTradedBaseAssetVolume { get; set; }
    
    [JsonProperty(PropertyName = "q")]
    public decimal TotalTradedQuoteAssetVolume { get; set; }
    
    [JsonProperty(PropertyName = "E")]
    public new long EventTime { get; set; }
}