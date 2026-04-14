namespace whobe.DTOs.Message;

public class CreateGroupConversationDto
{
    public string Name { get; set; } = null!;
    public List<Guid> MemberIds { get; set; } = new();
}
