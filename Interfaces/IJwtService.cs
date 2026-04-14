using System.Security.Claims;
using whobe.Models;

namespace whobe.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
