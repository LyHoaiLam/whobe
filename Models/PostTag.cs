namespace whobe.Models;

/// <summary>Tag một user khác vào bài viết (kiểu "cùng với @someone")</summary>
public class PostTag
{
    public long TagId { get; set; }
    public Guid PostId { get; set; }
    public Guid TaggedUserId { get; set; }
    public Guid TaggedById { get; set; }
    public bool IsApproved { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
    public User TaggedUser { get; set; } = null!;
    public User TaggedBy { get; set; } = null!;
}
