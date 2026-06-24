using whobe.DTOs.Common;
using whobe.DTOs.Notification;
using whobe.Enums;
namespace whobe.Interfaces;

public interface INotificationService {
    Task CreateAsync(Guid recipientId, Guid? actorId, NotificationType type, byte entityType, Guid entityId, string? message = null);
    Task<PagedResultDto<NotificationDto>> GetByUserAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(long notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(long notificationId, Guid userId);
}
