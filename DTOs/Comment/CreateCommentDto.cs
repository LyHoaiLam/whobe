namespace whobe.DTOs.Comment;

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
    public IFormFile? Media { get; set; }
}
