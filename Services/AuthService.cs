using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.DTOs.Auth;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;
//register, login, refresh token (lưu hash vào DB), logout, đổi mật khẩu
public class AuthService : IAuthService {
    private readonly AppDbContext _db;
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IUserRepository userRepo, IJwtService jwt, IConfiguration config) {
        _db = db;
        _userRepo = userRepo;
        _jwt = jwt;
        _config = config;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request) {
        if (await _userRepo.IsEmailExistsAsync(request.Email))
            throw new Exception("The email is already in use.");

        if (await _userRepo.IsUsernameExistsAsync(request.Username))
            throw new Exception("The username is already in use.");
        
        if (await _userRepo.IsPhoneNumberExistsAsync(request.PhoneNumber))
            throw new Exception("The phone number is already in use");

        var user = new User {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber.Trim(),
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return await GenerateAuthResponse(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request) {
        var user = await _userRepo.GetByEmailOrUsernameAsync(request.EmailOrUsername);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Incorrect username or password.");

        if (!user.IsActive)
            throw new Exception("The account has been disabled.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();

        return await GenerateAuthResponse(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken) {
        var tokenHash = HashToken(refreshToken);

        var userToken = await _db.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.TokenHash == tokenHash &&
                t.TokenType == 0 &&
                !t.IsRevoked &&
                t.ExpiresAt > DateTime.UtcNow);

        if (userToken == null)
            throw new Exception("The refresh token is invalid or has expired.");

        // Thu hồi token cũ
        userToken.IsRevoked = true;
        await _db.SaveChangesAsync();

        return await GenerateAuthResponse(userToken.User);
    }

    public async Task LogoutAsync(Guid userId) {
        var tokens = await _db.UserTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();

        foreach (var t in tokens) t.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword) {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            throw new Exception("The current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();
        return true;
    }

    // ── Private helpers ──────────────────────────────────

    private async Task<LoginResponseDto> GenerateAuthResponse(User user) {
        var accessToken  = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        var expiryDays   = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");

        // Lưu refresh token vào DB
        var userToken = new UserToken {
            UserId = user.UserId,
            TokenType = 0,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
        };

        await _db.UserTokens.AddAsync(userToken);
        await _db.SaveChangesAsync();

        return new LoginResponseDto {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60")),
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
        };
    }

    private static string HashToken(string token) {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
