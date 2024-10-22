using CryptoFinData.Core.DTOs;
using CryptoFinData.Core.Exeptions;
using CryptoFinData.Core.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace CryptoFinData.Infrastructure.External;

public class CoinDeskApiClient : ICryptoApiClient
{
    private readonly ICoinDeskApi _coinDeskApi;
    private readonly ILogger<CoinDeskApiClient> _logger;

    public CoinDeskApiClient(
        ICoinDeskApi coinDeskApi,
        ILogger<CoinDeskApiClient> logger)
    {
        _coinDeskApi = coinDeskApi ?? throw new ArgumentNullException(nameof(coinDeskApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CoinDeskResponseDto> GetCurrentPriceAsync()
    {
        try
        {
            var response = await _coinDeskApi.GetCurrentPriceAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error getting current price. Status code: {StatusCode}", response.StatusCode);
                throw new ApiException($"Error getting current price. Status code: {response.StatusCode}");
            }

            return response.Content;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error when getting current price");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when getting current price");
            throw new ApiException("Unexpected error when getting current price", ex);
        }
    }
}
