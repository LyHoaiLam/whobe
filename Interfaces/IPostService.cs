using whobe.DTOs.Common;
using whobe.DTOs.Post;
namespace whobe.Interfaces;

public interface IPostService {
    Task<PostResponseDto> CreatePostAsync(Guid userId, CreatePostDto dto);
    Task<PostResponseDto?> GetByIdAsync(Guid postId, Guid currentUserId);
    Task<PagedResultDto<PostResponseDto>> GetNewsFeedAsync(Guid userId, int page, int pageSize);
    Task<PagedResultDto<PostResponseDto>> GetUserPostsAsync(Guid profileUserId, Guid currentUserId, int page, int pageSize);
    Task<PostResponseDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostDto dto);
    Task DeletePostAsync(Guid postId, Guid userId);
    Task ReactToPostAsync(Guid userId, Guid postId, int reactionType);
    Task<PostResponseDto> SharePostAsync(Guid userId, Guid postId, string? caption);
}
