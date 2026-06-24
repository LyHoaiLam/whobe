namespace whobe.Models;

public class Story {
    public Guid StoryId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? MediaUrl { get; set; }

    // 0=Image, 1=Video, 2=Text
    public byte MediaType { get; set; }
    public string? TextContent { get; set; }

    // JSON string for font/color/position styling
    public string? TextStyle { get; set; }

    // 0=Public, 1=Friends, 2=OnlyMe
    public byte PrivacyLevel { get; set; } = 1;
    public int? DurationSeconds { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
}
