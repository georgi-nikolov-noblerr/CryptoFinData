namespace CryptoFinData.Core.DTOs;

public record UserDto(
    string Email,
    string Username,
    DateTime CreatedAt
);