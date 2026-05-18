using whobe.Models;
namespace whobe.Interfaces;

public interface ICommentRepository : IGenericRepository<Comment> {
    Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize);
    Task<IEnumerable<Comment>> GetRepliesAsync(Guid parentCommentId, int page, int pageSize);
    Task<Comment?> GetWithAuthorAsync(Guid commentId);
    Task<int> CountByPostAsync(Guid postId);
}
