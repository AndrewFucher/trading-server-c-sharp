namespace TradingServer.Client.Common.Exceptions;

public class NoCallbackConfiguredException : Exception
{
    public NoCallbackConfiguredException() : base("No callback configured for event type.")
    {
        
    }
    
    public NoCallbackConfiguredException(string message) : base(message)
    {
    }
}