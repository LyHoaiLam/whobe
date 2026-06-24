using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository {
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _set.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _set.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

    public async Task<User?> GetByEmailOrUsernameAsync(string value) =>
        await _set.FirstOrDefaultAsync(u => (u.Email == value || u.Username == value) && u.IsActive);

    public async Task<User?> GetProfileAsync(Guid userId) =>
        await _set.Include(u => u.Settings).FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<bool> IsEmailExistsAsync(string email) =>
        await _set.AnyAsync(u => u.Email == email);

    public async Task<bool> IsUsernameExistsAsync(string username) =>
        await _set.AnyAsync(u => u.Username == username);

    public async Task<IEnumerable<User>> SearchUsersAsync(string keyword, int page, int pageSize) =>
        await _set
            .Where(u => u.IsActive &&
                (u.FullName.Contains(keyword) || u.Username.Contains(keyword)))
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<User>> GetFriendsAsync(Guid userId) =>
        await _db.Friendships
            .Where(f => f.Status == Enums.FriendshipStatus.Accepted &&
                (f.RequesterId == userId || f.ReceiverId == userId))
            .Select(f => f.RequesterId == userId ? f.Receiver : f.Requester)
            .ToListAsync();

    public async Task<IEnumerable<User>> GetFollowersAsync(Guid userId) =>
        await _db.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower)
            .ToListAsync();

    public async Task<IEnumerable<User>> GetFollowingAsync(Guid userId) =>
        await _db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following)
            .ToListAsync();
}
