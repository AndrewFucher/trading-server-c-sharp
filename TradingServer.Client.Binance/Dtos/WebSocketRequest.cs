using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TradingServer.Client.Binance.Enums;
using TradingServer.Common;

namespace TradingServer.Client.Binance.Dtos;

public class WebSocketRequest
{
    [JsonProperty(PropertyName = "method")]
    [JsonConverter(typeof(StringEnumConverter))]
    public WebSocketRequestMethod Method { get; set; }

    [JsonProperty(PropertyName = "params")]
    public IEnumerable<object>? Params { get; set; }

    [JsonProperty(PropertyName = "id")]
    public uint Id { get; set; }
}