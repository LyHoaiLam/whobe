using whobe.Models;
namespace whobe.Interfaces;

public interface IPostRepository : IGenericRepository<Post> {
    Task<Post?> GetPostWithDetailsAsync(Guid postId);
    Task<IEnumerable<Post>> GetNewsFeedAsync(Guid userId, int page, int pageSize);
    Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize);
    Task<IEnumerable<Post>> GetGroupPostsAsync(Guid groupId, int page, int pageSize);
    Task<IEnumerable<Post>> GetPagePostsAsync(Guid pageId, int page, int pageSize);
    Task<IEnumerable<Post>> SearchPostsByHashtagAsync(string tag, int page, int pageSize);
    Task<int> CountReactionsAsync(Guid postId);
    Task<int> CountCommentsAsync(Guid postId);
}
