using whobe.Enums;

namespace whobe.Models;

public class Reaction
{
    public long ReactionId { get; set; }
    public Guid UserId { get; set; }

    // Generic target: 0=Post, 1=Comment, 2=Story
    public byte TargetType { get; set; }
    public Guid TargetId { get; set; }

    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
