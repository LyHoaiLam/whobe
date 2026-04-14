using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository {
    public PostRepository(AppDbContext db) : base(db) { }

    public async Task<Post?> GetPostWithDetailsAsync(Guid postId) =>
        await _set
            .Include(p => p.Author)
            .Include(p => p.Media)
            .Include(p => p.Feeling)
            .Include(p => p.Tags).ThenInclude(t => t.TaggedUser)
            .Include(p => p.PostHashtags).ThenInclude(ph => ph.Hashtag)
            .Include(p => p.OriginalPost).ThenInclude(op => op!.Author)
            .Include(p => p.OriginalPost).ThenInclude(op => op!.Media)
            .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

    public async Task<IEnumerable<Post>> GetNewsFeedAsync(Guid userId, int page, int pageSize) {
        // Lấy danh sách bạn bè + bản thân
        var friendIds = await _db.Friendships
            .Where(f => f.Status == Enums.FriendshipStatus.Accepted &&
                (f.RequesterId == userId || f.ReceiverId == userId))
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        friendIds.Add(userId);

        return await _set
            .Where(p => friendIds.Contains(p.AuthorId) &&
                !p.IsDeleted &&
                (p.Privacy == Enums.PostPrivacy.Public || p.Privacy == Enums.PostPrivacy.Friends))
            .Include(p => p.Author)
            .Include(p => p.Media)
            .Include(p => p.Feeling)
            .Include(p => p.OriginalPost).ThenInclude(op => op!.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize) =>
        await _set
            .Where(p => p.AuthorId == userId && !p.IsDeleted)
            .Include(p => p.Author)
            .Include(p => p.Media)
            .Include(p => p.Feeling)
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Post>> GetGroupPostsAsync(Guid groupId, int page, int pageSize) =>
        await _set
            .Where(p => p.GroupId == groupId && !p.IsDeleted)
            .Include(p => p.Author)
            .Include(p => p.Media)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Post>> GetPagePostsAsync(Guid pageId, int page, int pageSize) =>
        await _set
            .Where(p => p.PageId == pageId && !p.IsDeleted)
            .Include(p => p.Author)
            .Include(p => p.Media)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Post>> SearchPostsByHashtagAsync(string tag, int page, int pageSize) =>
        await _set
            .Where(p => !p.IsDeleted &&
                p.PostHashtags.Any(ph => ph.Hashtag.TagName == tag))
            .Include(p => p.Author)
            .Include(p => p.Media)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> CountReactionsAsync(Guid postId) =>
        await _db.Reactions
            .CountAsync(r => r.TargetId == postId && r.TargetType == 0);

    public async Task<int> CountCommentsAsync(Guid postId) =>
        await _db.Comments
            .CountAsync(c => c.PostId == postId && !c.IsDeleted);
}
