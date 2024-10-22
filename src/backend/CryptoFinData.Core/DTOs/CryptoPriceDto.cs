namespace CryptoFinData.Core.DTOs;

public record CryptoPriceDto(
    string Currency,
    decimal Price,
    decimal EurPrice,
    decimal GbpPrice,
    DateTime Timestamp,
    string Source
);
