namespace CryptoFinData.Core.DTOs;

public record CoinDeskResponseDto
{
    public TimeDto Time { get; init; }
    public string Disclaimer { get; init; }
    public string ChartName { get; init; }
    public BpiDto Bpi { get; init; }
}
