namespace whobe.Models;

public class Conversation {
    public Guid ConversationId { get; set; } = Guid.NewGuid();

    // 0=Direct(1-1), 1=Group
    public byte Type { get; set; } = 0;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? CreatedBy { get; set; }
    public long? LastMessageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? Creator { get; set; }
    public Message? LastMessage { get; set; }
    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    // NEW
    public string? DirectKey { get; set; }
}
