namespace whobe.DTOs.Message;

public class ConversationSummaryDto
{
    public Guid ConversationId { get; set; }
    public int Type { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public List<MemberSnapshotDto> Members { get; set; } = new();
}

public class MemberSnapshotDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}
