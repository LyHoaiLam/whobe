namespace whobe.DTOs.User;

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Gender { get; set; }
}
