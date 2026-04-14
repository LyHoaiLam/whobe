using whobe.Enums;

namespace whobe.Models;

public class Message
{
    public long MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public long? ReplyToId { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public Message? ReplyTo { get; set; }
    public ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();
}
