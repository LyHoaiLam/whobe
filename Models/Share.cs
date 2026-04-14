namespace whobe.Models;

public class Share
{
    public long ShareId { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }

    // Caption khi share
    public string? Caption { get; set; }

    // 0=Public, 1=Friends, 2=OnlyMe
    public byte PrivacyLevel { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
