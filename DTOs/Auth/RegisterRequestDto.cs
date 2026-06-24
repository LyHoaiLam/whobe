namespace whobe.DTOs.Auth;

public class RegisterRequestDto {
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
}
