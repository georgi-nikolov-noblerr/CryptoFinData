namespace CryptoFinData.Core.DTOs;

public record LoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);