using whobe.Enums;
namespace whobe.Models;

public class Friendship {
    public long FriendshipId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ReceiverId { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public User Requester { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
