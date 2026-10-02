using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.DTOs.Comment;
using whobe.DTOs.Common;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;
//comment + reply + react
public class CommentService : ICommentService {
    private readonly ICommentRepository _commentRepo;
    private readonly IPostRepository _postRepo;
    private readonly INotificationService _notifService;
    private readonly AppDbContext _db;

    public CommentService(ICommentRepository commentRepo, IPostRepository postRepo,
        INotificationService notifService, AppDbContext db) {
        _commentRepo = commentRepo;
        _postRepo = postRepo;
        _notifService = notifService;
        _db = db;
    }

    public async Task<CommentResponseDto> CreateAsync(Guid userId, Guid postId, CreateCommentDto dto) {
        var post = await _postRepo.GetByIdAsync(postId)
            ?? throw new Exception("Bài viết không tồn tại.");

        string? mediaUrl = null;
        if (dto.Media != null)
            mediaUrl = await UploadCommentMedia(dto.Media, postId);

        var comment = new Comment {
            PostId = postId,
            AuthorId = userId,
            Content  = dto.Content,
            MediaUrl = mediaUrl,
        };

        await _commentRepo.AddAsync(comment);
        await _commentRepo.SaveChangesAsync();

        if (post.AuthorId != userId)
            await _notifService.CreateAsync(
                post.AuthorId, userId,
                Enums.NotificationType.Comment, 0, postId);

        var full = await _commentRepo.GetWithAuthorAsync(comment.CommentId);
        return MapToDto(full!);
    }

    public async Task<CommentResponseDto> ReplyAsync(Guid userId, Guid parentCommentId, CreateCommentDto dto) {
        var parent = await _commentRepo.GetWithAuthorAsync(parentCommentId)
            ?? throw new Exception("Comment không tồn tại.");

        string? mediaUrl = null;
        if (dto.Media != null)
            mediaUrl = await UploadCommentMedia(dto.Media, parent.PostId);

        var reply = new Comment {
            PostId = parent.PostId,
            AuthorId = userId,
            ParentCommentId = parentCommentId,
            Content = dto.Content,
            MediaUrl = mediaUrl,
        };

        await _commentRepo.AddAsync(reply);
        await _commentRepo.SaveChangesAsync();

        if (parent.AuthorId != userId)
            await _notifService.CreateAsync(
                parent.AuthorId, userId,
                Enums.NotificationType.Comment, 1, parentCommentId);

        var full = await _commentRepo.GetWithAuthorAsync(reply.CommentId);
        return MapToDto(full!);
    }

    public async Task<PagedResultDto<CommentResponseDto>> GetByPostAsync(Guid postId, int page, int pageSize) {
        var comments = await _commentRepo.GetByPostIdAsync(postId, page, pageSize);
        var total = await _commentRepo.CountByPostAsync(postId);
        return new PagedResultDto<CommentResponseDto> {
            Items = comments.Select(MapToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<PagedResultDto<CommentResponseDto>> GetRepliesAsync(Guid commentId, int page, int pageSize) {
        var replies = await _commentRepo.GetRepliesAsync(commentId, page, pageSize);
        return new PagedResultDto<CommentResponseDto> {
            Items = replies.Select(MapToDto),
            TotalCount = replies.Count(),
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<CommentResponseDto> UpdateAsync(Guid commentId, Guid userId, string content) {
        var comment = await _commentRepo.GetWithAuthorAsync(commentId)
            ?? throw new Exception("Comment không tồn tại.");

        if (comment.AuthorId != userId)
            throw new Exception("Bạn không có quyền chỉnh sửa comment này.");

        comment.Content   = content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentRepo.SaveChangesAsync();
        return MapToDto(comment);
    }

    public async Task DeleteAsync( Guid commentId, Guid userId) {
        var comment = await _commentRepo.GetByIdAsync(commentId)
            ?? throw new Exception("Comment không tồn tại.");

        if (comment.AuthorId != userId)
            throw new Exception("Bạn không có quyền xóa comment này.");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentRepo.SaveChangesAsync();
    }

    public async Task ReactAsync(Guid userId, Guid commentId, int reactionType) {
        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.UserId == userId &&
            r.TargetId == commentId && r.TargetType == 1);

        if (existing != null) {
            if ((int)existing.ReactionType == reactionType)
                _db.Reactions.Remove(existing);
            else
                existing.ReactionType = (Enums.ReactionType)reactionType;
        }
        else {
            _db.Reactions.Add(new Reaction {
                UserId = userId,
                TargetType = 1,
                TargetId = commentId,
                ReactionType = (Enums.ReactionType)reactionType,
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task<string?> UploadCommentMedia(IFormFile file, Guid postId) {
        // Reuse FileStorageService nếu cần — để đơn giản gọi thẳng
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        // Trong thực tế inject IFileStorageService; ở đây placeholder
        return null; // TODO: inject & use _storage.UploadAsync(file, "comments")
    }

    private static CommentResponseDto MapToDto(Comment c) => new() {
        CommentId = c.CommentId,
        PostId = c.PostId,
        AuthorId = c.AuthorId,
        AuthorName = c.Author?.FullName ?? "",
        AuthorAvatar = c.Author?.AvatarUrl,
        ParentCommentId = c.ParentCommentId,
        Content = c.Content,
        MediaUrl = c.MediaUrl,
        ReactionCount = c.Reactions?.Count ?? 0,
        ReplyCount = c.Replies?.Count(r => !r.IsDeleted) ?? 0,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
