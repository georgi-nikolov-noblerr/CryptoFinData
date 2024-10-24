using Bogus;
using CryptoFinData.Core.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CryptoFinData.UnitTests.Infrastructure.Services
{
    public class CryptoPriceServiceTests
    {
        private readonly Mock<ICryptoApiClient> _mockApiClient;
        private readonly Mock<ILogger<CryptoPriceService>> _mockLogger;
        private readonly CryptoPriceService _service;
        private readonly Faker _faker;

        public CryptoPriceServiceTests()
        {
            _mockApiClient = new Mock<ICryptoApiClient>();
            _mockLogger = new Mock<ILogger<CryptoPriceService>>();
            _service = new CryptoPriceService(_mockApiClient.Object, _mockLogger.Object);
            _faker = new Faker();
        }

        [Fact]
        public async Task GetCurrentPriceAsync_WhenApiCallSucceeds_ReturnsFormattedPrice()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var mockResponse = new CoinDeskResponseDto
            {
                Time = new TimeDto
                {
                    Updated = currentTime.ToString("MMM d, yyyy HH:mm:ss UTC"),
                    UpdatedISO = currentTime.ToString("O"),
                    UpdatedUK = currentTime.ToString("MMM d, yyyy HH:mm:ss 'GMT'")
                },
                Disclaimer = "This data was produced from the CoinDesk Bitcoin Price Index",
                ChartName = "Bitcoin",
                Bpi = new BpiDto
                {
                    USD = new CurrencyRate
                    {
                        Code = "USD",
                        Symbol = "&#36;",
                        Rate = "35,000.50",
                        Description = "United States Dollar",
                        Rate_Float = 35000.50m
                    },
                    EUR = new CurrencyRate
                    {
                        Code = "EUR",
                        Symbol = "&euro;",
                        Rate = "32,000.75",
                        Description = "Euro",
                        Rate_Float = 32000.75m
                    },
                    GBP = new CurrencyRate
                    {
                        Code = "GBP",
                        Symbol = "&pound;",
                        Rate = "28,000.25",
                        Description = "British Pound Sterling",
                        Rate_Float = 28000.25m
                    }
                }
            };

            _mockApiClient
                .Setup(x => x.GetCurrentPriceAsync())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.GetCurrentPriceAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new CryptoPriceDto(
                Currency: "BTC",
                Price: 35000.50m,
                EurPrice: 32000.75m,
                GbpPrice: 28000.25m,
                Timestamp: DateTime.Parse(mockResponse.Time.UpdatedISO),
                Source: "CoinDesk"
            ));
        }

        [Fact]
        public async Task GetCurrentPriceAsync_WhenApiCallFails_LogsAndThrowsException()
        {
            // Arrange
            var expectedError = new Exception("API Error");
            _mockApiClient
                .Setup(x => x.GetCurrentPriceAsync())
                .ThrowsAsync(expectedError);

            // Act & Assert
            await _service.Invoking(s => s.GetCurrentPriceAsync())
                .Should().ThrowAsync<Exception>()
                .Where(ex => ex == expectedError);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedError,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetHistoricalPricesAsync_WithValidDateRange_ReturnsCorrectNumberOfPrices()
        {
            // Arrange
            var from = DateTime.UtcNow.Date.AddDays(-5);
            var to = DateTime.UtcNow.Date;
            var expectedDays = 6; // inclusive of both dates

            // Act
            var results = await _service.GetHistoricalPricesAsync(from, to);

            // Assert
            var resultsList = results.ToList();
            resultsList.Should().HaveCount(expectedDays);
            resultsList.Should().BeInAscendingOrder(x => x.Timestamp);
            
            // Verify date range
            resultsList.First().Timestamp.Date.Should().Be(from.Date);
            resultsList.Last().Timestamp.Date.Should().Be(to.Date);

            // Verify each price record
            resultsList.Should().AllSatisfy(price =>
            {
                price.Should().BeOfType<CryptoPriceDto>();
                price.Currency.Should().Be("BTC");
                price.Price.Should().BeInRange(25000m, 45000m);
                price.EurPrice.Should().BeInRange(23000m, 42000m);
                price.GbpPrice.Should().BeInRange(21000m, 45000m);
                price.Source.Should().Be("Historical Data");
            });
        }

        [Fact]
        public async Task GetHistoricalPricesAsync_WithInvalidDateRange_ReturnsEmptyCollection()
        {
            // Arrange
            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(-1);

            // Act
            var results = await _service.GetHistoricalPricesAsync(from, to);

            // Assert
            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        public async Task GetHistoricalPricesAsync_WithDifferentRanges_ReturnsCorrectNumberOfDays(int days)
        {
            // Arrange
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(days);
            var expectedDays = days + 1; // inclusive of both dates

            // Act
            var results = await _service.GetHistoricalPricesAsync(from, to);

            // Assert
            results.Should().HaveCount(expectedDays);
            results.Should().BeInAscendingOrder(x => x.Timestamp);
            
            // Verify each record has the correct date
            var resultsList = results.ToList();
            for (int i = 0; i < resultsList.Count; i++)
            {
                resultsList[i].Timestamp.Date.Should().Be(from.AddDays(i).Date);
            }
        }
    }
}
