using System.ComponentModel.DataAnnotations;
namespace whobe.Models;

public class Feeling {
    public int FeelingId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(10)]
    public string? Emoji { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    // Navigation
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
