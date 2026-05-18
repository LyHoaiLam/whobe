using whobe.DTOs.Comment;
using whobe.DTOs.Common;
namespace whobe.Interfaces;

public interface ICommentService {
    Task<CommentResponseDto> CreateAsync(Guid userId, Guid postId, CreateCommentDto dto);
    Task<CommentResponseDto> ReplyAsync(Guid userId, Guid parentCommentId, CreateCommentDto dto);
    Task<PagedResultDto<CommentResponseDto>> GetByPostAsync(Guid postId, int page, int pageSize);
    Task<PagedResultDto<CommentResponseDto>> GetRepliesAsync(Guid commentId, int page, int pageSize);
    Task<CommentResponseDto> UpdateAsync(Guid commentId, Guid userId, string content);
    Task DeleteAsync(Guid commentId, Guid userId);
    Task ReactAsync(Guid userId, Guid commentId, int reactionType);
}
