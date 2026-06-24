namespace whobe.DTOs.User;

/// <summary>
/// DTO riêng cho danh sách lời mời kết bạn đang chờ — khác UserProfileDto
/// vì cần thêm field FriendshipId để FE gọi accept/decline.
/// </summary>
public class PendingRequestDto
{
    public long FriendshipId { get; set; }   // BẮT BUỘC — FE cần để accept/decline
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

