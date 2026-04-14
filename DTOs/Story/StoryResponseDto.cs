namespace whobe.DTOs.Story;

public class StoryResponseDto
{
    public Guid StoryId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserAvatar { get; set; }
    public string? MediaUrl { get; set; }
    public int MediaType { get; set; }
    public string? TextContent { get; set; }
    public int ViewCount { get; set; }
    public bool IsViewed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
