namespace whobe.Models;

public class PostHashtag {
    public Guid PostId { get; set; }
    public long HashtagId { get; set; }
    public Post Post { get; set; } = null!;
    public Hashtag Hashtag { get; set; } = null!;
}
