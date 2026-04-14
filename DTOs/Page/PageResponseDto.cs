namespace whobe.DTOs.Page;

public class PageResponseDto
{
    public Guid PageId { get; set; }
    public string Name { get; set; } = null!;
    public string? Username { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public int FollowerCount { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
