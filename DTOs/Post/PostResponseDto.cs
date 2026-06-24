namespace whobe.DTOs.Post;

public class PostResponseDto {
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public string? AuthorAvatar { get; set; }
    public string? Content { get; set; }
    public int Privacy { get; set; }
    public int ReactionCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int? MyReaction { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public bool IsPinned { get; set; }
    public bool IsShared { get; set; }
    public PostResponseDto? OriginalPost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
