namespace whobe.DTOs.Message;

public class SendMessageDto {
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public int MessageType { get; set; } = 0;
    // public IFormFile? Media { get; set; }
    public long? ReplyToId { get; set; }
}

