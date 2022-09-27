namespace TradingServer.Common;

public static class RandomUtils
{
    private static readonly Random Random = new Random();
    
    public static uint NextUInt()
    {
        var thirtyBits = (uint) Random.Next(1 << 30);
        var twoBits = (uint) Random.Next(1 << 2);
        var fullRange = (thirtyBits << 2) | twoBits;
        return fullRange;
    }
}