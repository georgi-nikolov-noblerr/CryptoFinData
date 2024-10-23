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
                Timestamp: DateTime.UtcNow,
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
            if (from > to) return Task.FromResult(Enumerable.Empty<CryptoPriceDto>());
            var totalDays = (to - from).Days + 1;
            var mockData = new List<CryptoPriceDto>();
            for (int i = 0; i < totalDays; i++)
            {
                var currentDate = from.AddDays(i);

                var priceDto = _faker.Generate() with
                {
                    Timestamp = currentDate, 
                    Price = _faker.Generate().Price,  
                    EurPrice = _faker.Generate().Price,
                    GbpPrice = _faker.Generate().Price
                };

                mockData.Add(priceDto);
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
