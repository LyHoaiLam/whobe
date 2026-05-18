namespace whobe.Models;

public class BlockList {
    public long BlockId { get; set; }
    public Guid BlockerId { get; set; }
    public Guid BlockedId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Blocker { get; set; } = null!;
    public User Blocked { get; set; } = null!;
}
