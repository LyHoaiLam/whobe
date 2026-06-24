using whobe.DTOs.Auth;
namespace whobe.Interfaces;

public interface IAuthService {
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
}
