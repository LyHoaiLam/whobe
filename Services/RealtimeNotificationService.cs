using Microsoft.AspNetCore.SignalR;
using whobe.DTOs.Notification;
using whobe.Hubs;
using whobe.Interfaces;

namespace whobe.Services;

public class RealtimeNotificationService : IRealtimeNotificationService {
    private readonly IHubContext<NotificationHub> _hub;

    public RealtimeNotificationService(IHubContext<NotificationHub> hub) =>
        _hub = hub;

    public async Task PushAsync(Guid userId, NotificationDto notification) =>
        // Clients.Group(userId) => push đến tất cả thiết bị của user đó
        await _hub.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification);

    public async Task PushUnreadCountAsync(Guid userId, int count) =>
        await _hub.Clients
            .Group(userId.ToString())
            .SendAsync("UnreadCount", count);
}
