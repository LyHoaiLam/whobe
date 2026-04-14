using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Enums;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository {
    public FriendshipRepository(AppDbContext db) : base(db) { }

    public async Task<Friendship?> GetAsync(Guid userAId, Guid userBId) =>
        await _set.FirstOrDefaultAsync(f =>
            (f.RequesterId == userAId && f.ReceiverId == userBId) ||
            (f.RequesterId == userBId && f.ReceiverId == userAId));

    public async Task<FriendshipStatus?> GetStatusAsync(Guid userAId, Guid userBId) {
        var f = await GetAsync(userAId, userBId);
        return f?.Status;
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId) =>
        await _set
            .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
            .Include(f => f.Requester)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Friendship>> GetFriendListAsync(Guid userId) =>
        await _set
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                (f.RequesterId == userId || f.ReceiverId == userId))
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .ToListAsync();

    public async Task<bool> AreFriendsAsync(Guid userAId, Guid userBId) =>
        await _set.AnyAsync(f =>
            f.Status == FriendshipStatus.Accepted &&
            ((f.RequesterId == userAId && f.ReceiverId == userBId) ||
             (f.RequesterId == userBId && f.ReceiverId == userAId)));

    public async Task<int> CountFriendsAsync(Guid userId) =>
        await _set.CountAsync(f =>
            f.Status == FriendshipStatus.Accepted &&
            (f.RequesterId == userId || f.ReceiverId == userId));

    public async Task<IEnumerable<User>> GetMutualFriendsAsync(Guid userAId, Guid userBId) {
        var aFriendIds = await _set
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                (f.RequesterId == userAId || f.ReceiverId == userAId))
            .Select(f => f.RequesterId == userAId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        var bFriendIds = await _set
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                (f.RequesterId == userBId || f.ReceiverId == userBId))
            .Select(f => f.RequesterId == userBId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        var mutualIds = aFriendIds.Intersect(bFriendIds).ToList();

        return await _db.Users
            .Where(u => mutualIds.Contains(u.UserId))
            .ToListAsync();
    }
}
