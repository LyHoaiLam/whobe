namespace whobe.Models;

public class Group {
    public Guid GroupId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }

    // 0=Public, 1=Private, 2=Secret
    public byte Privacy { get; set; } = 0;
    public string? Category { get; set; }
    public int MemberCount { get; set; } = 0;
    public Guid OwnerId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
