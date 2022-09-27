using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TradingServer.Client.Binance.Abstractions;
using TradingServer.Client.Binance.Dtos;
using TradingServer.Client.Binance.Enums;
using TradingServer.Client.Common.Exceptions;
using TradingServer.Client.WebSocketWrapper;

namespace TradingServer.Client.Binance.WebSocket;

/// <inheritdoc />
public class BinanceSpotWebSocketManager : IBinanceSpotWebSocketManager
{
    public readonly IDictionary<EventType, Action<WebSocketEvent>> Callbacks =
        new ConcurrentDictionary<EventType, Action<WebSocketEvent>>();

    private readonly ILogger<BinanceSpotWebSocketManager> _logger;
    private readonly BinanceWebSocketWrapper _binanceWebSocketWrapper;

    private readonly IDictionary<uint, WebSocketResponse> _webSocketResponses =
        new ConcurrentDictionary<uint, WebSocketResponse>();

    private readonly JsonSerializerSettings _jsonSerializerSettings =
        new() {NullValueHandling = NullValueHandling.Ignore};

    private IEnumerable<string> _subscriptions = new ConcurrentBag<string>();
    private bool _shouldUpdateSubscriptions = false;
    private bool _working = false;

    private uint _id = 1;

    public BinanceSpotWebSocketManager(ILogger<BinanceSpotWebSocketManager> logger)
    {
        _logger = logger;
        _binanceWebSocketWrapper = new BinanceWebSocketWrapper(logger, HandleMessageEvent);
    }

    public async Task StartAsync()
    {
        await _binanceWebSocketWrapper.StartAsync();
        _working = true;
    }

    public async Task<WebSocketResponse> SendRequest(WebSocketRequest request)
    {
        if (!_working)
            throw new NotStartedWebSocketException();

        request.Id = _id++;

        if (request.Method is WebSocketRequestMethod.Subscribe or WebSocketRequestMethod.Unsubscribe)
            _shouldUpdateSubscriptions = true;

        var message = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
        _logger.LogInformation("Sending {}", message);
        _binanceWebSocketWrapper.Send(message);

        while (!_webSocketResponses.ContainsKey(request.Id))
        {
            await Task.Delay(100);
        }

        _webSocketResponses.Remove(request.Id, out var response);

        return response!;
    }

    // public async Task<IEnumerable<string>> SubscribeToMiniTicker24Hour(string symbol)
    // {
    //     return await SubscribeToMiniTicker24Hour(symbol);
    // }

    public async Task<IEnumerable<string>> SubscribeToMiniTicker24Hour(params string[] symbols)
    {
        return await SubscribeToMiniTicker24Hour(symbols.ToList());
    }

    public async Task<IEnumerable<string>> SubscribeToMiniTicker24Hour(IEnumerable<string> symbols)
    {
        if (!Callbacks.ContainsKey(EventType.MiniTicker24Hour))
            throw new NoCallbackConfiguredException($"No callback configured for Binance event type {EventType.MiniTicker24Hour}");
        
        const string streamName = "miniTicker";

        var request = new WebSocketRequest
        {
            Method = WebSocketRequestMethod.Subscribe,
            Params = symbols.Select(s => $"{s}@{streamName}")
        };

        await SendRequest(request);
        return (await GetSubscriptions()).Where(s => s.Contains($"{streamName}"));
    }

    public async Task SubscribeToAllMiniTicker24Hour()
    {
        if (!Callbacks.ContainsKey(EventType.MiniTicker24Hour))
            throw new NoCallbackConfiguredException($"No callback configured for Binance event type {EventType.MiniTicker24Hour}");
        
        const string streamName = "!miniTicker@arr";

        var request = new WebSocketRequest
        {
            Method = WebSocketRequestMethod.Subscribe,
            Params = new[] {streamName}
        };

        await SendRequest(request);
    }

    public async Task<IEnumerable<string>> GetSubscriptions()
    {
        if (!_shouldUpdateSubscriptions)
            return _subscriptions;

        var request = new WebSocketRequest
        {
            Method = WebSocketRequestMethod.ListSubscriptions
        };

        var response = JsonConvert.SerializeObject((await SendRequest(request)).Result);
        var subscriptions = JsonConvert.DeserializeObject<IEnumerable<string>>(response)!;
        _subscriptions = subscriptions;

        _shouldUpdateSubscriptions = false;

        return _subscriptions;
    }

    private void HandleMessageEvent(object? sender, MessageEvent messageEvent)
    {
        try
        {
            if (messageEvent.Message.Contains("id"))
                HandleWebSocketResponse(messageEvent.Message);
            else if (messageEvent.Message.Contains("\"e\""))
                HandleWebSocketEvent(messageEvent.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Error Message: {}, Error: {}\n{}", e.Message, e.Data, e);
        }
    }

    private void HandleWebSocketResponse(string message)
    {
        var response = JsonConvert.DeserializeObject<WebSocketResponse>(message)!;
        _webSocketResponses.Add(response.Id, response);
    }

    private void HandleWebSocketEvent(string @event)
    {
        if (@event.Contains("24hrMiniTicker"))
        {
            HandleWebSocketMiniTickerEvent(@event);
        }
    }

    private void HandleWebSocketMiniTickerEvent(string @event)
    {
        if (!Callbacks.TryGetValue(EventType.MiniTicker24Hour, out var action))
        {
            _logger.LogWarning("No callback specified for event type {}", EventType.MiniTicker24Hour);
            return;
        }

        if (@event.StartsWith("{"))
        {
            var parsedEvent = JsonConvert.DeserializeObject<MiniTickerWebSocketEvent>(@event)!;
            action.Invoke(parsedEvent);

            return;
        }

        var parsedEvents = JsonConvert.DeserializeObject<IEnumerable<MiniTickerWebSocketEvent>>(@event)!;
        foreach (var miniTickerWebSocketEvent in parsedEvents)
            action.Invoke(miniTickerWebSocketEvent);
    }
}