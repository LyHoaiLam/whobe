namespace whobe.Models;

public class SavedPost
{
    public long SavedId { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
