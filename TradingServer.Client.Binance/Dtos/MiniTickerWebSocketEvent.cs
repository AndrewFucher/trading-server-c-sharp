using Newtonsoft.Json;

namespace TradingServer.Client.Binance.Dtos;

public class MiniTickerWebSocketEvent : WebSocketEvent
{
    [JsonProperty(PropertyName = "c")]
    public int ClosePrice { get; set; }
    
    [JsonProperty(PropertyName = "o")]
    public int OpenPrice { get; set; }
    
    [JsonProperty(PropertyName = "h")]
    public int HighPrice { get; set; }
    
    [JsonProperty(PropertyName = "l")]
    public int LowPrice { get; set; }
    
    [JsonProperty(PropertyName = "v")]
    public int TotalTradedBaseAssetVolume { get; set; }
    
    [JsonProperty(PropertyName = "q")]
    public int TotalTradedQuoteAssetVolume { get; set; }
}