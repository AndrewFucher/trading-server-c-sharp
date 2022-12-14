using System.Collections.Concurrent;
using System.Reactive.Linq;
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
    private readonly IDictionary<EventType, Func<WebSocketEvent, Task>> _callbacks =
        new ConcurrentDictionary<EventType, Func<WebSocketEvent, Task>>();

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

    public IBinanceSpotWebSocketManager AddCallback(EventType key, Func<WebSocketEvent, Task> value)
    {
        if (_callbacks.TryGetValue(key, out var action))
            _callbacks[key] += value;
        else
            _callbacks.Add(key, value);

        return this;
    }

    public IBinanceSpotWebSocketManager RemoveCallback(EventType key, Func<WebSocketEvent, Task> value)
    {
        if (_callbacks.ContainsKey(key))
            _callbacks[key] -= value;

        return this;
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
        // _logger.LogInformation($"Sending {message}");
        await _binanceWebSocketWrapper.SendAsync(message);

        while (!_webSocketResponses.ContainsKey(request.Id))
        {
            await Task.Delay(100).ConfigureAwait(false);
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
        if (!_callbacks.ContainsKey(EventType.MiniTicker24Hour))
            throw new NoCallbackConfiguredException(
                $"No callback configured for Binance event type {EventType.MiniTicker24Hour}");

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
        if (!_callbacks.ContainsKey(EventType.MiniTicker24Hour))
            throw new NoCallbackConfiguredException(
                $"No callback configured for Binance event type {EventType.MiniTicker24Hour}");

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

    private async Task HandleMessageEvent(WebSocketMessage webSocketMessage)
    {
        try
        {
            if (webSocketMessage.Message.Contains("id"))
            {
                HandleWebSocketResponse(webSocketMessage.Message);
            }
            else if (webSocketMessage.Message.Contains("\"e\""))
            {
                await HandleWebSocketEvent(webSocketMessage.Message);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error Message: {e.Message}, Error: {e.Data}\n{e.ToString()}");
        }
    }

    private void HandleWebSocketResponse(string message)
    {
        var response = JsonConvert.DeserializeObject<WebSocketResponse>(message)!;
        _webSocketResponses.Add(response.Id, response);
    }

    private async Task HandleWebSocketEvent(string @event)
    {
        if (@event.Contains("24hrMiniTicker"))
        {
            await HandleWebSocketMiniTickerEvent(@event);
        }
    }

    private Task HandleWebSocketMiniTickerEvent(string @event)
    {
        if (!_callbacks.TryGetValue(EventType.MiniTicker24Hour, out var action) || action == null)
        {
            _logger.LogWarning($"No callback specified for event type {EventType.MiniTicker24Hour}");
            return Task.CompletedTask;
        }

        if (@event.StartsWith("{"))
        {
            var parsedEvent = JsonConvert.DeserializeObject<MiniTickerWebSocketEvent>(@event)!;
            return action.Invoke(parsedEvent);
        }

        var parsedEvents = JsonConvert.DeserializeObject<IEnumerable<MiniTickerWebSocketEvent>>(@event)!;
        return Task.WhenAll(parsedEvents.Select(v => action.Invoke(v)));
        // foreach (var miniTickerWebSocketEvent in parsedEvents)
        //     action.Invoke(miniTickerWebSocketEvent);
        // return Task.CompletedTask;
    }
}