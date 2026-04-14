namespace whobe.Models;

public class Page
{
    public Guid PageId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? Username { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int FollowerCount { get; set; } = 0;
    public Guid OwnerId { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
