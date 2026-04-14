namespace whobe.DTOs.User;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public bool IsVerified { get; set; }
    public int FriendCount { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public string? FriendshipStatus { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
