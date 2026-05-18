using whobe.Models;
namespace whobe.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification> {
    Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, int page, int pageSize);
    Task<int> CountUnreadAsync(Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task MarkAsReadAsync(long notificationId);
}
