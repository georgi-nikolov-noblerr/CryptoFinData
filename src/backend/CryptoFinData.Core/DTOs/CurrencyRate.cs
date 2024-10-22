namespace CryptoFinData.Core.DTOs;

public record CurrencyRate
{
    public string Code { get; init; }
    public string Symbol { get; init; }
    public string Rate { get; init; }
    public string Description { get; init; }
    public decimal Rate_Float { get; init; }
}