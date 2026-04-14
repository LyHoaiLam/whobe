namespace whobe.Models;

public class PostMedia
{
    public long MediaId { get; set; }
    public Guid PostId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public int MediaType { get; set; } // 0=Image, 1=Video, 2=File
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; }
    public int? FileSizeKb { get; set; }
    public byte SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; } = null!;
}
