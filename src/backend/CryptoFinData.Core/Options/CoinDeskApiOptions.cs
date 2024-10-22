namespace CryptoFinData.Core.Options;

public class CoinDeskApiOptions
{
    public const string SectionName = "CoinDeskApi";
    public string BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
