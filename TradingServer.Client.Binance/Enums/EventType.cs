using System.Runtime.Serialization;

namespace TradingServer.Client.Binance.Enums;

public enum EventType
{
    [EnumMember(Value = "24hrMiniTicker")]
    MiniTicker24Hour,
    [EnumMember(Value = "kline")]
    KLine,
    [EnumMember(Value = "trade")]
    Trade,
    [EnumMember(Value = "aggTrade")]
    AggTrade,
    [EnumMember(Value = "24hrTicker")]
    Ticker24Hour,
    [EnumMember(Value = "1hTicker")]
    Ticker1Hour,
    [EnumMember(Value = "4hTicker")]
    Ticker4Hour,
    [EnumMember(Value = "depthUpdate")]
    DepthUpdate,
    [EnumMember(Value = "partialDepth")]
    PartialDepth,
    [EnumMember(Value = "bookTicker")]
    BookTicker
}