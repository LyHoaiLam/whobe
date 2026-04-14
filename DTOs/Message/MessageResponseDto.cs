namespace whobe.DTOs.Message;

public class MessageResponseDto
{
    public long MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? SenderAvatar { get; set; }
    public int MessageType { get; set; }
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public long? ReplyToId { get; set; }
    public string? ReplyToContent { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
}
