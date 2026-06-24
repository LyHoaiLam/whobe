using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class StoryRepository : GenericRepository<Story>, IStoryRepository {
    public StoryRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Story>> GetActiveStoriesOfUserAsync(Guid userId) =>
        await _set
            .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Story>> GetFriendsStoriesAsync(Guid userId) {
        var friendIds = await _db.Friendships
            .Where(f => f.Status == Enums.FriendshipStatus.Accepted &&
                (f.RequesterId == userId || f.ReceiverId == userId))
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        friendIds.Add(userId);

        return await _set
            .Where(s => friendIds.Contains(s.UserId) && s.ExpiresAt > DateTime.UtcNow)
            .Include(s => s.User)
            .Include(s => s.Views)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasViewedAsync(Guid storyId, Guid viewerId) =>
        await _db.StoryViews.AnyAsync(v => v.StoryId == storyId && v.ViewerId == viewerId);

    public async Task<int> CountViewsAsync(Guid storyId) =>
        await _db.StoryViews.CountAsync(v => v.StoryId == storyId);
}
