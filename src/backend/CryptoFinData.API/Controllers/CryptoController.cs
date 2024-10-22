using CryptoFinData.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/crypto")]
public class CryptoController : ControllerBase
{
    private readonly ICryptoPriceService _cryptoPriceService;
    private readonly ILogger<CryptoController> _logger;

    public CryptoController(
        ICryptoPriceService cryptoPriceService,
        ILogger<CryptoController> logger)
    {
        _cryptoPriceService = cryptoPriceService;
        _logger = logger;
    }

    [HttpGet("price")]
    public async Task<ActionResult<CryptoPriceDto>> GetCurrentPrice()
    {
        var price = await _cryptoPriceService.GetCurrentPriceAsync();
        return Ok(price);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<CryptoPriceDto>>> GetHistoricalPrices(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var prices = await _cryptoPriceService.GetHistoricalPricesAsync(from, to);
        return Ok(prices);
    }
}
