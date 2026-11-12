// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using whobe.Data;
// using whobe.DTOs.Auth;
// using whobe.Models;
// using Microsoft.AspNetCore.Authorization;
// using System.Security.Claims;
// using whobe.Interfaces;

// namespace whobe.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class AuthController : ControllerBase {
//     private readonly AppDbContext _context;
//     private readonly IJwtService _jwtService;

//     public AuthController(AppDbContext context, IJwtService jwtService) {
//         _context = context;
//         _jwtService = jwtService;
//     }

// // =========================
// // REGISTER API
// // =========================
//     [HttpPost("register")]
//     // public async Task<IActionResult> Register(RegisterRequestDto dto) {
//     //     var usernameExists = await _context.Users.AnyAsync(x => x.Username == dto.Username);

//     //     if (usernameExists)
//     //         return BadRequest(new { status = 400, message = "The username already exists" });

//     //     var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);

//     //     if (emailExists)
//     //         return BadRequest(new { status = 400, message = "The email already exists." });

//     //     var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

//     //     var user = new User {
//     //         Username = dto.Username,
//     //         Email = dto.Email,
//     //         PhoneNumber = dto.PhoneNumber,
//     //         PasswordHash = passwordHash,
//     //         FullName = dto.FullName,
//     //         DateOfBirth = dto.DateOfBirth
//     //     };

//     //     _context.Users.Add(user);
//     //     await _context.SaveChangesAsync();

//     //     return Ok( new {
//     //         status = 201,
//     //         message = "Registration successful",
//     //         // userId = user.UserId
//     //     });
//     // }
    

// // =========================
// // LOGIN API
// // =========================
//     [HttpPost("login")]
//     public async Task<IActionResult> Login(LoginRequestDto dto) {
//         // 1. tìm user theo username hoặc email
//         var user = await _context.Users
//             .FirstOrDefaultAsync(x => x.Username == dto.EmailOrUsername || x.Email == dto.EmailOrUsername);

//         if (user == null) {
//             return Unauthorized(new {
//                 status = 401,
//                 message = "The account does not exist."
//             });
//         }

//         // 2. check password
//         var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

//         if (!isValidPassword) {
//             return Unauthorized(new {
//                 status = 401,
//                 message = "Incorrect password"
//             });
//         }

//         // 3. generate tokens
//         var accessToken = _jwtService.GenerateAccessToken(user);
//         var refreshToken = _jwtService.GenerateRefreshToken();

//         // 4. lưu refresh token (nếu bạn dùng bảng UserTokens)
//         _context.UserTokens.Add(new UserToken {
//             UserId = user.UserId,
//             TokenHash = refreshToken,
//             TokenType = 1, // refresh token
//             ExpiresAt = DateTime.UtcNow.AddDays(7),
//             CreatedAt = DateTime.UtcNow,
//             IsRevoked = false
//         });

//         await _context.SaveChangesAsync();

//         // 5. response
//         return Ok(new LoginResponseDto {
//             AccessToken = accessToken,
//             RefreshToken = refreshToken,
//             ExpiresAt = DateTime.UtcNow.AddMinutes(60),
//             UserId = user.UserId,
//             Username = user.Username,
//             FullName = user.FullName,
//             AvatarUrl = user.AvatarUrl
//         });
//     }


// // =========================
// // GET ME (require JWT)
// // =========================
//     [Authorize]
//     [HttpGet("infor")]
//         public async Task<IActionResult> GetMyInfor() {
//             // Lấy userId từ token
//             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//             if (userIdClaim == null) {
//                 return Unauthorized(new {
//                     status = 401,
//                     message = "Token không hợp lệ"
//                 });
//             }

//             var userId = Guid.Parse(userIdClaim);

//             var user = await _context.Users
//                 .Where(x => x.UserId == userId)
//                 .Select(x => new {
//                     x.UserId,
//                     x.Username,
//                     x.Email,
//                     x.FullName,
//                     x.PhoneNumber,
//                     x.AvatarUrl,
//                     x.CoverPhotoUrl,
//                     x.Bio,
//                     x.Location,
//                     x.DateOfBirth,
//                     x.CreatedAt
//                 })
//                 .FirstOrDefaultAsync();

//             if (user == null) {
//                 return NotFound(new {
//                     status = 404,
//                     message = "Không tìm thấy user"
//                 });
//             }

//             return Ok(new {
//                 status = 200,
//                 data = user
//             });
//         }

// }




using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.DTOs.Auth;
using whobe.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using whobe.Interfaces;

namespace whobe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IJwtService jwtService, IAuthService authService) {
        _context = context;
        _jwtService = jwtService;
        _authService = authService;
    }

// =========================
// REGISTER API
// =========================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto) {
        try {
            var result = await _authService.RegisterAsync(dto);

            return StatusCode(StatusCodes.Status201Created, new {
                status = 201,
                message = "Registration successful",
                data = result
            });
        } catch (Exception ex) {
            return BadRequest(new {
                status = 400,
                message = ex.Message
            });
        }
    }


// =========================
// LOGIN API
// =========================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto) {
        try {
            var result = await _authService.LoginAsync(dto);

            return Ok(new {
                status = 200,
                message = "Login successful",
                data = result
            });
        } catch (Exception ex) {
            return Unauthorized(new {
                status = 401,
                message = ex.Message
            });
        }
    }



// =========================
// GET ME (require JWT)
// =========================
    [Authorize]
    [HttpGet("infor")]
    public async Task<IActionResult> GetMyInfor() {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null) {
            return Unauthorized(new {
                status = 401,
                message = "Invalid token"
            });
        }

        var userId = Guid.Parse(userIdClaim);

        var user = await _context.Users
            .Where(x => x.UserId == userId)
            .Select(x => new {
                x.UserId,
                x.Username,
                x.Email,
                x.FullName,
                x.PhoneNumber,
                x.AvatarUrl,
                x.CoverPhotoUrl,
                x.Bio,
                x.Location,
                x.DateOfBirth,
                x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null) {
            return NotFound(new {
                status = 404,
                message = "User cannot be found"
            });
        }

        return Ok(new {
            status = 200,
            data = user
        });
    }

}