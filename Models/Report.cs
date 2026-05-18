namespace whobe.Models;

public class Report {
    public long ReportId { get; set; }
    public Guid ReporterId { get; set; }

    // 0=Post, 1=Comment, 2=User, 3=Story, 4=Message
    public byte TargetType { get; set; }
    public Guid TargetId { get; set; }

    // 0=Spam, 1=Harassment, 2=HateSpeech, 3=Violence, 4=FakeInfo, 5=Other
    public byte ReasonCode { get; set; }
    public string? Description { get; set; }

    // 0=Pending, 1=Reviewed, 2=Resolved, 3=Dismissed
    public byte Status { get; set; } = 0;
    public Guid? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public User Reporter { get; set; } = null!;
    public User? Reviewer { get; set; }
}
