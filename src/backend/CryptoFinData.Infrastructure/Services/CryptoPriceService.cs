using Bogus;
using CryptoFinData.Core.DTOs;
using Microsoft.Extensions.Logging;

public class CryptoPriceService : ICryptoPriceService
{
    private readonly ICryptoApiClient _apiClient;
    private readonly ILogger<CryptoPriceService> _logger;
    private readonly Faker<CryptoPriceDto> _faker;

    public CryptoPriceService(
        ICryptoApiClient apiClient,
        ILogger<CryptoPriceService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;

        _faker = new Faker<CryptoPriceDto>()
            .CustomInstantiator(f => new CryptoPriceDto(
                Currency: "BTC",
                Price: f.Random.Decimal(25000m, 45000m),
                EurPrice: f.Random.Decimal(23000m, 42000m),
                GbpPrice: f.Random.Decimal(21000m, 39000m),
                Timestamp: f.Date.Recent(days: 30),
                Source: "Historical Data"
            ));
    }

    public async Task<CryptoPriceDto> GetCurrentPriceAsync()
    {
        try
        {
            var apiResponse = await _apiClient.GetCurrentPriceAsync();
            
            var price = new CryptoPriceDto(
                Currency: "BTC",
                Price: apiResponse.Bpi.USD.Rate_Float,
                EurPrice: apiResponse.Bpi.EUR.Rate_Float,
                GbpPrice: apiResponse.Bpi.GBP.Rate_Float,
                Timestamp: DateTime.Parse(apiResponse.Time.UpdatedISO),
                Source: "CoinDesk"
            );

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current price from API");
            throw;
        }
    }

    public Task<IEnumerable<CryptoPriceDto>> GetHistoricalPricesAsync(DateTime from, DateTime to)
    {
        try
        {
            var days = (to - from).Days;
            if (days <= 0) return Task.FromResult(Enumerable.Empty<CryptoPriceDto>());

            var mockData = _faker
                .Generate(days)
                .OrderBy(p => p.Timestamp)
                .ToList();

            var currentIndex = 0;
            var currentDate = from;
            while (currentDate <= to)
            {
                if (currentIndex < mockData.Count)
                {
                    var basePrice = mockData[currentIndex].Price;
                    mockData[currentIndex] = mockData[currentIndex] with
                    {
                        Timestamp = currentDate,
                        Price = basePrice + (decimal)(Math.Sin(currentDate.Hour * 0.5) * 1000),
                        EurPrice = basePrice * 0.92m + (decimal)(Math.Sin(currentDate.Hour * 0.5) * 900),
                        GbpPrice = basePrice * 0.85m + (decimal)(Math.Sin(currentDate.Hour * 0.5) * 800)
                    };
                }
                currentDate = currentDate.AddDays(1);
                currentIndex++;
            }

            return Task.FromResult<IEnumerable<CryptoPriceDto>>(mockData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating historical prices");
            throw;
        }
    }
}
