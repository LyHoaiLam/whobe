namespace whobe.DTOs.Story;

public class CreateStoryDto
{
    public IFormFile? Media { get; set; }
    public int MediaType { get; set; } = 0;
    public string? TextContent { get; set; }
    public string? TextStyle { get; set; }
    public byte PrivacyLevel { get; set; } = 1;
}
