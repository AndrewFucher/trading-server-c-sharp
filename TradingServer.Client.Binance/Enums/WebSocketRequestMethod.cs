using System.Runtime.Serialization;

namespace TradingServer.Client.Binance.Enums;

public enum WebSocketRequestMethod
{
    [EnumMember(Value = "SUBSCRIBE")]
    Subscribe,
    [EnumMember(Value = "UNSUBSCRIBE")]
    Unsubscribe,
    [EnumMember(Value = "LIST_SUBSCRIPTIONS")]
    ListSubscriptions,
    [EnumMember(Value = "SET_PROPERTY")]
    SetProperty
}