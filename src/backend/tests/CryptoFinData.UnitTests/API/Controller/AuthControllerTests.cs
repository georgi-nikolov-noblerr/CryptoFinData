using CryptoFinData.API.Controllers;
using CryptoFinData.Core.DTOs;
using CryptoFinData.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CryptoFinData.UnitTests.API.Controller;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequestDto("testuser", "password123");
        var expectedResponse = new LoginResponseDto(
            Token: "jwt-token",
            ExpiresAt: DateTime.UtcNow.AddHours(24),
            User: new UserDto("test@example.com", "testuser", DateTime.UtcNow)
        );

        _authServiceMock
            .Setup(x => x.LoginAsync(loginRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var loginResponse = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;

        loginResponse.Should().BeEquivalentTo(expectedResponse);
        
        _authServiceMock.Verify(x => x.LoginAsync(loginRequest), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto("invaliduser", "wrongpassword");

        _authServiceMock
            .Setup(x => x.LoginAsync(loginRequest))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var responseValue = unauthorizedResult.Value as Response;
        responseValue?.message.Should().Be("Invalid username or password");

        _authServiceMock.Verify(x => x.LoginAsync(loginRequest), Times.Once);
    }

    [Fact]
    public async Task Login_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var loginRequest = new LoginRequestDto("testuser", "password123");
        var expectedException = new Exception("Service error");

        _authServiceMock
            .Setup(x => x.LoginAsync(loginRequest))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);

        var responseValue = statusCodeResult.Value as Response;
        responseValue?.message.Should().Be("An error occurred during login");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null, "password123")]
    [InlineData("", "password123")]
    [InlineData("testuser", null)]
    [InlineData("testuser", "")]
    public async Task Login_WithInvalidRequest_ReturnsBadRequest(string username, string password)
    {
        // Arrange
        var loginRequest = new LoginRequestDto(username, password);

        _authServiceMock
            .Setup(x => x.LoginAsync(loginRequest))
            .ThrowsAsync(new ArgumentException("Invalid request"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private record Response(string message);
}
