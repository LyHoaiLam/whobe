namespace whobe.Models;

public class GroupMember {
    public long GmId { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }

    // 0=Member, 1=Moderator, 2=Admin
    public byte Role { get; set; } = 0;

    // 0=Active, 1=Pending, 2=Banned
    public byte Status { get; set; } = 0;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
}
