namespace whobe.Models;

public class StoryView {
    public long ViewId { get; set; }
    public Guid StoryId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    // null = chỉ xem, không react; giá trị = ReactionType
    public byte? ReactionType { get; set; }

    // Navigation
    public Story Story { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}
