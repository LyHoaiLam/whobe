using System.ComponentModel.DataAnnotations;
using whobe.Enums;
namespace whobe.Models;

public class User {
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required, MaxLength(255)]
    public string Email { get; set; } = null!;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }
    public UserGender Gender { get; set; } = UserGender.Unspecified;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(500)]
    public string? CoverPhotoUrl { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? Website { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // 0=Public, 1=Friends, 2=OnlyMe
    public byte PrivacyLevel { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public UserSettings? Settings { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Friendship> SentRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedRequests { get; set; } = new List<Friendship>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();
    public ICollection<BlockList> Blocked { get; set; } = new List<BlockList>();
    public ICollection<BlockList> BlockedBy { get; set; } = new List<BlockList>();
}
