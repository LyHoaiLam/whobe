namespace whobe.DTOs.Message;

public class ConversationDto {
    public Guid ConversationId { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    public MessageResponseDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class ConversationMemberDto {
    public Guid UserId { get; set; }
    public string Username  { get; set; } = null!;
    public string FullName  { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}
