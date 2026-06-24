using whobe.Enums;
namespace whobe.Models;

public class Notification {
    public long NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid? ActorId { get; set; }
    public NotificationType Type { get; set; }

    // 0=Post, 1=Comment, 2=User, 3=Message, 4=Story
    public byte EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public User Recipient { get; set; } = null!;
    public User? Actor { get; set; }
}
