namespace CryptoFinData.Core.DTOs;

public record BpiDto
{
    public CurrencyRate USD { get; init; }
    public CurrencyRate GBP { get; init; }
    public CurrencyRate EUR { get; init; }
}
