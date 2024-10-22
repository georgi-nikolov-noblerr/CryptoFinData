using CryptoFinData.Core.DTOs;

namespace CryptoFinData.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
