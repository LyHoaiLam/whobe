using whobe.DTOs.Notification;

namespace whobe.Interfaces;

/// <summary>
/// Push thông báo realtime đến client qua SignalR.
/// Tách riêng khỏi INotificationService (lưu DB)
/// để tránh circular dependency.
/// </summary>
public interface IRealtimeNotificationService {
    /// <summary>Push 1 thông báo đến đúng userId</summary>
    Task PushAsync(Guid userId, NotificationDto notification);

    /// <summary>Push số lượng thông báo chưa đọc (badge)</summary>
    Task PushUnreadCountAsync(Guid userId, int count);
}
