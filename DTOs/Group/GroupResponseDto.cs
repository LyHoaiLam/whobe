namespace whobe.DTOs.Group;

public class GroupResponseDto
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public int Privacy { get; set; }
    public string? Category { get; set; }
    public int MemberCount { get; set; }
    public bool IsMember { get; set; }
    public int? MyRole { get; set; }
    public DateTime CreatedAt { get; set; }
}
