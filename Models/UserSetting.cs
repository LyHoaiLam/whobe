namespace whobe.Models;

public class UserSettings
{
    public long SettingId { get; set; }
    public Guid UserId { get; set; }
    public string Language { get; set; } = "vi";
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public bool NotifyLikes { get; set; } = true;
    public bool NotifyComments { get; set; } = true;
    public bool NotifyFriendRequests { get; set; } = true;
    public bool NotifyMessages { get; set; } = true;
    public bool NotifyMentions { get; set; } = true;

    // 0=Public, 1=Friends, 2=OnlyMe
    public byte ProfileVisibility { get; set; } = 0;
    public byte WhoCanMessage { get; set; } = 1;
    public byte WhoCanSeeFollowers { get; set; } = 0;

    public User User { get; set; } = null!;
}
