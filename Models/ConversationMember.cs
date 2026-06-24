namespace whobe.Models;

public class ConversationMember {
    public long MemberId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }

    // 0=Member, 1=Admin, 2=Owner
    public byte Role { get; set; } = 0;
    public string? Nickname { get; set; }
    public DateTime? LastReadAt { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsMuted { get; set; } = false;

    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
