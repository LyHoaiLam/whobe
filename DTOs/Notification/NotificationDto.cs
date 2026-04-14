namespace whobe.DTOs.Notification;

public class NotificationDto
{
    public long NotificationId { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? ActorAvatar { get; set; }
    public int Type { get; set; }
    public int EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
