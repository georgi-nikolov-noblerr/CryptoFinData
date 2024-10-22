using CryptoFinData.Core.DTOs;
using Refit;

namespace CryptoFinData.Core.Interfaces.External;

public interface ICoinDeskApi
{
    [Get("/currentprice.json")]
    Task<ApiResponse<CoinDeskResponseDto>> GetCurrentPriceAsync();
}
