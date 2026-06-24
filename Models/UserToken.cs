namespace whobe.Models;

public class UserToken {
    public long TokenId { get; set; }
    public Guid UserId { get; set; }

    // 0=RefreshToken, 1=EmailVerification, 2=PasswordReset
    public byte TokenType { get; set; }
    public string TokenHash { get; set; } = null!;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    public User User { get; set; } = null!;
}
