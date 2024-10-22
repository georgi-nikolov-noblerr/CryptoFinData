namespace CryptoFinData.Core.DTOs;

public record LoginRequestDto(
    string Username,
    string Password
);