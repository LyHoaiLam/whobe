using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository {
    public NotificationRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, int page, int pageSize) =>
        await _set
            .Where(n => n.RecipientId == userId)
            .Include(n => n.Actor)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> CountUnreadAsync(Guid userId) =>
        await _set.CountAsync(n => n.RecipientId == userId && !n.IsRead);

    public async Task MarkAllAsReadAsync(Guid userId) {
        var unread = await _set
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread) {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(long notificationId) {
        var n = await _set.FindAsync(notificationId);
        if (n != null) {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
