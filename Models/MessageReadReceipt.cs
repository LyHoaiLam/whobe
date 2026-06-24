namespace whobe.Models;

public class MessageReadReceipt {
    public long ReceiptId { get; set; }
    public long MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    public Message Message { get; set; } = null!;
    public User User { get; set; } = null!;
}
