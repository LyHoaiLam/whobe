namespace whobe.DTOs.Comment;

public class CommentResponseDto {
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorAvatar { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public string? MediaUrl { get; set; }
    public int ReactionCount { get; set; }
    public int ReplyCount { get; set; }
    public int? MyReaction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
