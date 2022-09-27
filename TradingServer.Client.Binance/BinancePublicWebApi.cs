using Pathoschild.Http.Client;

namespace TradingServer.Client.Binance;

public class BinancePublicWebApi
{
    private readonly IClient _webClient;

    public BinancePublicWebApi(string binanceApiUrl = "https://api.binance.com")
    {
        _webClient = new FluentClient(binanceApiUrl)
            .SetOptions(ignoreNullArguments: true);
    }

    public void GetExchangeInfo(IEnumerable<string>? symbols = null, IEnumerable<string>? permissions = null)
    {
        permissions = permissions ?? new[] {"SPOT"};
        
        // _webClient.GetAsync("/api/v3/exchangeInfo")
        //     .WithArguments(new {symbols = symbols, permissions = permissions})
        //     .As<>();
    }
}