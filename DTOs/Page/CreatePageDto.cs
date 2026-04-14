namespace whobe.DTOs.Page;

public class CreatePageDto
{
    public string Name { get; set; } = null!;
    public string? Username { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public IFormFile? Avatar { get; set; }
}
