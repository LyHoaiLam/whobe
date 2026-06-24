using whobe.DTOs.Common;
using whobe.DTOs.Notification;
using whobe.Enums;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;

public class NotificationService : INotificationService {
    private readonly INotificationRepository _notifRepo;
    private readonly IRealtimeNotificationService _realtime;

    public NotificationService(INotificationRepository notifRepo, IRealtimeNotificationService realtime) {
        _notifRepo = notifRepo;
        _realtime  = realtime;
    }

    public async Task CreateAsync(
        Guid recipientId, Guid? actorId,
        NotificationType type, byte entityType,
        Guid entityId, string? message = null) {
        // Không tự thông báo cho chính mình
        if (actorId.HasValue && actorId.Value == recipientId) return;

        var notif = new Notification {
            RecipientId = recipientId,
            ActorId = actorId,
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            Message = message,
        };

        await _notifRepo.AddAsync(notif);
        await _notifRepo.SaveChangesAsync();

        // ── Push realtime ngay sau khi lưu DB ──────────────
        var dto = MapToDto(notif);
        await _realtime.PushAsync(recipientId, dto);

        // Cập nhật badge (số thông báo chưa đọc)
        var unread = await _notifRepo.CountUnreadAsync(recipientId);
        await _realtime.PushUnreadCountAsync(recipientId, unread);
    }

    public async Task<PagedResultDto<NotificationDto>> GetByUserAsync(Guid userId, int page, int pageSize) {
        var list  = await _notifRepo.GetByUserAsync(userId, page, pageSize);
        var total = await _notifRepo.CountUnreadAsync(userId);

        return new PagedResultDto<NotificationDto> {
            Items = list.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _notifRepo.CountUnreadAsync(userId);

    public async Task MarkAsReadAsync(long notificationId, Guid userId) =>
        await _notifRepo.MarkAsReadAsync(notificationId);

    public async Task MarkAllAsReadAsync(Guid userId) {
        await _notifRepo.MarkAllAsReadAsync(userId);

        // Reset badge về 0
        await _realtime.PushUnreadCountAsync(userId, 0);
    }

    public async Task DeleteAsync(long notificationId, Guid userId) {
        var notif = await _notifRepo.GetByIdAsync(notificationId);
        if (notif != null && notif.RecipientId == userId) {
            await _notifRepo.DeleteAsync(notif);
            await _notifRepo.SaveChangesAsync();
        }
    }

    private static NotificationDto MapToDto(Notification n) => new() {
        NotificationId = n.NotificationId,
        ActorId = n.ActorId,
        ActorName = n.Actor?.FullName,
        ActorAvatar = n.Actor?.AvatarUrl,
        Type = (int)n.Type,
        EntityType = n.EntityType,
        EntityId = n.EntityId,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
    };
}
