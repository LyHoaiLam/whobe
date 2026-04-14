namespace whobe.DTOs.Story;

public class StoryGroupDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserAvatar { get; set; }
    public bool HasUnviewed { get; set; }
    public List<StoryResponseDto> Stories { get; set; } = new();
}
