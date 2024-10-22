using CryptoFinData.Core.DTOs;

public interface ICryptoApiClient
{
    Task<CoinDeskResponseDto> GetCurrentPriceAsync();
}
