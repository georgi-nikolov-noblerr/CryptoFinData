using CryptoFinData.Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CryptoFinData.UnitTests.API.Controller
{
    public class CryptoControllerTests
    {
        private readonly Mock<ICryptoPriceService> _mockCryptoPriceService;
        private readonly CryptoController _controller;

        public CryptoControllerTests()
        {
            _mockCryptoPriceService = new Mock<ICryptoPriceService>();
            Mock<ILogger<CryptoController>> mockLogger = new();
            _controller = new CryptoController(
                _mockCryptoPriceService.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task GetCurrentPrice_ReturnsOkResultWithPrice()
        {
            // Arrange
            var expectedPrice = new CryptoPriceDto(
                Currency: "BTC",
                Price: 35000.50m,
                EurPrice: 32000.75m,
                GbpPrice: 28000.25m,
                Timestamp: DateTime.UtcNow,
                Source: "CoinDesk"
            );

            _mockCryptoPriceService
                .Setup(x => x.GetCurrentPriceAsync())
                .ReturnsAsync(expectedPrice);

            // Act
            var result = await _controller.GetCurrentPrice();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPrice = okResult.Value.Should().BeOfType<CryptoPriceDto>().Subject;
            
            returnedPrice.Should().BeEquivalentTo(expectedPrice);
        }

        [Fact]
        public async Task GetHistoricalPrices_WithValidDateRange_ReturnsOkResultWithPrices()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-7);
            var to = DateTime.UtcNow;
            
            var expectedPrices = new List<CryptoPriceDto>
            {
                new(
                    Currency: "BTC",
                    Price: 35000.50m,
                    EurPrice: 32000.75m,
                    GbpPrice: 28000.25m,
                    Timestamp: from,
                    Source: "Historical Data"
                ),
                new(
                    Currency: "BTC",
                    Price: 36000.50m,
                    EurPrice: 33000.75m,
                    GbpPrice: 29000.25m,
                    Timestamp: to,
                    Source: "Historical Data"
                )
            };

            _mockCryptoPriceService
                .Setup(x => x.GetHistoricalPricesAsync(from, to))
                .ReturnsAsync(expectedPrices);

            // Act
            var result = await _controller.GetHistoricalPrices(from, to);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPrices = okResult.Value.Should().BeOfType<List<CryptoPriceDto>>().Subject;
            
            returnedPrices.Should().BeEquivalentTo(expectedPrices);
        }

        [Fact]
        public async Task GetHistoricalPrices_WithInvalidDateRange_ReturnsOkWithEmptyList()
        {
            // Arrange
            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(-7); // Invalid range (from > to)

            _mockCryptoPriceService
                .Setup(x => x.GetHistoricalPricesAsync(from, to))
                .ReturnsAsync(Array.Empty<CryptoPriceDto>());

            // Act
            var result = await _controller.GetHistoricalPrices(from, to);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPrices = okResult.Value.Should().BeOfType<CryptoPriceDto[]>().Subject;
            
            returnedPrices.Should().BeEmpty();
        }

        [Theory]
        [InlineData(-7, 0)]  
        [InlineData(-30, 0)] 
        [InlineData(-1, 1)]
        public async Task GetHistoricalPrices_WithVariousDateRanges_ReturnsOkResult(int fromDays, int toDays)
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(fromDays);
            var to = DateTime.UtcNow.AddDays(toDays);
            
            var expectedPrices = new List<CryptoPriceDto>
            {
                new(
                    Currency: "BTC",
                    Price: 35000.50m,
                    EurPrice: 32000.75m,
                    GbpPrice: 28000.25m,
                    Timestamp: from,
                    Source: "Historical Data"
                )
            };

            _mockCryptoPriceService
                .Setup(x => x.GetHistoricalPricesAsync(from, to))
                .ReturnsAsync(expectedPrices);

            // Act
            var result = await _controller.GetHistoricalPrices(from, to);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockCryptoPriceService.Verify(
                x => x.GetHistoricalPricesAsync(from, to),
                Times.Once
            );
        }
    }
}
