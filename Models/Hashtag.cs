namespace whobe.Models;

public class Hashtag
{
    public long HashtagId { get; set; }
    public string TagName { get; set; } = null!;
    public int PostCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
}
