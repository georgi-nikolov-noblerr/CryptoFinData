using FluentAssertions;
using Bogus;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using CryptoFinData.Core.DTOs;
using CryptoFinData.Core.Entities;

namespace CryptoFinData.UnitTests.Infrastructure.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly Faker _faker;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-256-bit-secret-key-here-minimum-32-characters");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("your-issuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("your-audience");

            Mock<ILogger<AuthService>> mockLogger = new();

            _authService = new AuthService(
                _context,
                _mockConfiguration.Object,
                mockLogger.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var password = _faker.Internet.Password();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            
            var user = new User(
                email: _faker.Internet.Email(),
                username: _faker.Internet.UserName(),
                passwordHash: passwordHash
            );
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDto(
                Username: user.Username,
                Password: password
            );

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            result.User.Should().NotBeNull();
            result.User.Username.Should().Be(user.Username);
            result.User.Email.Should().Be(user.Email);

            // Verify JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(result.Token);
            
            jwtToken.Claims.First(c => c.Type == "unique_name").Value.Should().Be(user.Username);
            jwtToken.Claims.First(c => c.Type == "nameid").Value.Should().Be(user.Id.ToString());
        }

        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new LoginRequestDto(
                Username: "nonexistent-user",
                Password: "any-password"
            );

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginRequest))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var correctPassword = _faker.Internet.Password();
            var user = new User(
                email: _faker.Internet.Email(),
                username: _faker.Internet.UserName(),
                passwordHash: BCrypt.Net.BCrypt.HashPassword(correctPassword)
            );
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDto(
                Username: user.Username,
                Password: "wrong-password"
            );

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginRequest))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password");
        }

        [Fact]
        public async Task LoginAsync_WithMissingJwtKey_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns((string)null);

            var password = _faker.Internet.Password();
            var user = new User(
                email: _faker.Internet.Email(),
                username: _faker.Internet.UserName(),
                passwordHash: BCrypt.Net.BCrypt.HashPassword(password)
            );
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDto(
                Username: user.Username,
                Password: password
            );

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginRequest))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("JWT Key is not configured");
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("user", "")]
        [InlineData(null, "password")]
        [InlineData("user", null)]
        public async Task LoginAsync_WithInvalidInput_ThrowsArgumentException(string username, string password)
        {
            // Arrange
            var loginRequest = new LoginRequestDto(
                Username: username,
                Password: password
            );

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginRequest))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
