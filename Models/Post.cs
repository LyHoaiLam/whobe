using whobe.Enums;

namespace whobe.Models;

public class Post
{
    public Guid PostId { get; set; } = Guid.NewGuid();
    public Guid AuthorId { get; set; }
    public Guid? PageId { get; set; }
    public Guid? GroupId { get; set; }
    public string? Content { get; set; }
    public PostPrivacy Privacy { get; set; } = PostPrivacy.Public;

    // Share: trỏ về post gốc
    public Guid? OriginalPostId { get; set; }
    public int ShareCount { get; set; } = 0;

    // Location
    public string? LocationName { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Feeling (FK -> Feelings)
    public int? FeelingId { get; set; }

    public bool IsPinned { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Author { get; set; } = null!;
    public Page? Page { get; set; }
    public Group? Group { get; set; }
    public Feeling? Feeling { get; set; }
    public Post? OriginalPost { get; set; }
    public ICollection<Post> Shares { get; set; } = new List<Post>();
    public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
    public ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<SavedPost> SavedBy { get; set; } = new List<SavedPost>();
}
