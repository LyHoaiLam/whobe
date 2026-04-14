namespace whobe.DTOs.Post;

public class CreatePostDto
{
    public string? Content { get; set; }
    public int Privacy { get; set; } = 0;
    public Guid? GroupId { get; set; }
    public Guid? PageId { get; set; }
    public List<IFormFile>? Media { get; set; }
    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
