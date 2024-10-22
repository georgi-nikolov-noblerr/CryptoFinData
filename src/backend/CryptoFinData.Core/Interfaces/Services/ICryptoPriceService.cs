using CryptoFinData.Core.DTOs;

public interface ICryptoPriceService
{
    Task<CryptoPriceDto> GetCurrentPriceAsync();
    Task<IEnumerable<CryptoPriceDto>> GetHistoricalPricesAsync(DateTime from, DateTime to);
}
