using Newtonsoft.Json;
using TradingServer.Client.Binance.Enums;

namespace TradingServer.Client.Binance.Dtos;

public class WebSocketEvent
{
    [JsonProperty(PropertyName = "e")]
    public EventType EventType { get; set; }
    [JsonProperty(PropertyName = "E")]
    public long? EventTime { get; set; }
    [JsonProperty(PropertyName = "s")]
    public string? Symbol { get; set; }
}