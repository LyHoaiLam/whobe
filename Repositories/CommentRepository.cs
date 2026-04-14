using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository {
    public CommentRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize) =>
        await _set
            .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Comment>> GetRepliesAsync(Guid parentCommentId, int page, int pageSize) =>
        await _set
            .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<Comment?> GetWithAuthorAsync(Guid commentId) =>
        await _set
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

    public async Task<int> CountByPostAsync(Guid postId) =>
        await _set.CountAsync(c => c.PostId == postId && !c.IsDeleted);
}
