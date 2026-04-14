namespace whobe.DTOs.Story;

public class StoryViewerDto
{
    public Guid ViewerId { get; set; }
    public string ViewerName { get; set; } = null!;
    public string? ViewerAvatar { get; set; }
    public byte? ReactionType { get; set; }
    public DateTime ViewedAt { get; set; }
}
