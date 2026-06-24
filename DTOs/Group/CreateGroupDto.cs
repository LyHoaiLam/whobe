namespace whobe.DTOs.Group;

public class CreateGroupDto {
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public byte Privacy { get; set; } = 0;
    public string? Category { get; set; }
    public IFormFile? Avatar { get; set; }
}
