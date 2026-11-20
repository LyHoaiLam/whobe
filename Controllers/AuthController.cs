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

    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

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
        var usernameExists = await _context.Users.AnyAsync(x => x.Username == dto.Username);

        if (usernameExists)
            return BadRequest(new { status = 400, message = "The username already exists" });

        var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);

        if (emailExists)
            return BadRequest(new { status = 400, message = "The email already exists." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = passwordHash,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok( new {
            status = 201,
            message = "Registration successful",
            // userId = user.UserId
        });
    }
    

// =========================
// LOGIN API
// =========================
    [HttpPost("login")]
    // public async Task<IActionResult> Login(LoginRequestDto dto) {
    //     // 1. tìm user theo username hoặc email
    //     var user = await _context.Users
    //         .FirstOrDefaultAsync(x => x.Username == dto.EmailOrUsername || x.Email == dto.EmailOrUsername);

    //     if (user == null) {
    //         return Unauthorized(new {
    //             status = 401,
    //             message = "The account does not exist."
    //         });
    //     }

    //     // 2. check password
    //     var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

    //     if (!isValidPassword) {
    //         return Unauthorized(new {
    //             status = 401,
    //             message = "Incorrect password"
    //         });
    //     }

    //     // 3. generate tokens
    //     var accessToken = _jwtService.GenerateAccessToken(user);
    //     var refreshToken = _jwtService.GenerateRefreshToken();

    //     // 4. lưu refresh token (nếu bạn dùng bảng UserTokens)
    //     _context.UserTokens.Add(new UserToken {
    //         UserId = user.UserId,
    //         TokenHash = refreshToken,
    //         TokenType = 1, // refresh token
    //         ExpiresAt = DateTime.UtcNow.AddDays(7),
    //         CreatedAt = DateTime.UtcNow,
    //         IsRevoked = false
    //     });

    //     await _context.SaveChangesAsync();

    //     // 5. response
    //     return Ok(new LoginResponseDto {
    //         AccessToken = accessToken,
    //         RefreshToken = refreshToken,
    //         ExpiresAt = DateTime.UtcNow.AddMinutes(60),
    //         UserId = user.UserId,
    //         Username = user.Username,
    //         FullName = user.FullName,
    //         AvatarUrl = user.AvatarUrl
    //     });
    // }

    public async Task<IActionResult> Login(LoginRequestDto dto) {
        try {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (Exception ex) {
            return Unauthorized(new {
                status = 401,
                message = ex.Message
            });
        }
    }



    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto) {
        try {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken)) {
                return BadRequest(new {
                    status = 400,
                    message = "Refresh token is required."
                });
            }

            var result = await _authService.RefreshTokenAsync(
                dto.RefreshToken
            );

            return Ok(result);
        }
        catch (Exception ex) {
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
            // Lấy userId từ token
            //Lấy UserId của user hiện đang login từ thông tin xác thực (Claims) được lưu trong JWT Token
            // Để hiểu rõ, chúng ta cần hiểu 3 khái niệm: User, Claim và ClaimTypes.NameIdentifier.
            // Để hiểu rõ, chúng ta cần tìm hiểu 3 khái niệm: User, Claim và ClaimIdentifier.



            // 1. Claim là gì
            // Trong ASP.NET Core Authentication, Claim là một mẩu thông tin (key-value) dùng để mô tả danh tính hoặc quyền hạn của một người dùng.

            // Ví dụ, sau khi đăng nhập thành công, Backend tạo JWT Token chứa các thông tin:
            // {
            //     "sub": "a1b2c3d4-1234-5678-9012-abcdef123456",
            //     "nameid": "a1b2c3d4-1234-5678-9012-abcdef123456",
            //     "unique_name": "hoailam",
            //     "email": "lam@gmail.com",
            //     "role": "Admin"
            // }

            // Các thông tin trên được gọi là Claims
            // ======================== Ví dụ ========================
            // Claim                       Giá trị                        Ý nghĩa
            // NamIdentifier               a1b2c3d4...                    ID user
            // Name                        hoailam                        Tên user
            // Email                       lam@gmail.com                  Email
            // Role                        Admin                          Vai trò

            // Lưu ý: Đây là ví dụ minh họa. Token thực tế có thể chứa các Claim khác nhau tùy cách bạn cấu hình.



            // 2. ClaimTypes.NameIdentifier là gì?
            // ClaimTypes.NameIdentifier là một hằng số có sẵn trong .NET, thuộc namespace: using System.Security.Claims;
            // Nó đại diện cho loại Claim dùng để xác định danh tính duy nhất của một người dùng.
            // Bạn có thể hiểu đơn giản:
            // ClaimTypes: Danh sách các loại Claim được .NET định nghĩa sẵn.
            // NameIdentifier: Loại Claim đại diện cho ID định danh của người dùng.

            // Ví dụ khi tạo JWT trong

            // var claims = new List<Claim>
            // {
            //     new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            //     new Claim(ClaimTypes.Name, user.Username),
            //     new Claim(ClaimTypes.Email, user.Email)
            // };
            // Ở trên, chúng ta đang đưa UserId, Username và Email vào danh sách Claims để tạo token



            // 3. Phân tích dòng code "var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;"
            // Chúng ta tách từng phần:
            // Thành phần                               Ý nghĩa
            // User                                     Đối tượng ClaimsPrincipal đại diện cho user đang gửi request
            // FindFirst()                              Tìm Claim đầu tiên có loại được chỉ định
            // ClaimTypes.NameIdentifier                Loại Claim chứa ID user
            // ?.Value                                  Nếu tìm thấy Claim thì lấy value; nếu không thì trả về null
           
            // Ví dụ:
            // Giả sử người dùng đăng nhập và JWT Token chứa:
            // {
            //     "nameid": "a1b2c3d4-1234-5678-9012-abcdef123456",
            //     "unique_name": "hoailam"
            // }
            // Khi gửi request:
                // GET /api/users/infor
                // Authorization: Bearer <JWT_TOKEN>

            // Middleware Authentication của ASP.NET Core sẽ xác thực Token và xây dựng đối tượng User chứa các Claims tương ứng.
            // Khi thực thi: var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Kết quả: userIdClaim = "a1b2c3d4-1234-5678-9012-abcdef123456";

            // Tiếp theo
            // var userId = Guid.Parse(userIdClaim);
            // Chuyển chuỗi thành kiểu dữ liệu Guid để truy vấn Database.

            
            
            // 4. Tại sao không lấy UserId từ Frontend gửi lên?
            // Đây là điểm rất quan trọng trong thiết kế Backend.
            // Giả sử API của bạn viết như sau:

            // [HttpGet("infor/{userId}")]
            //     public async Task<IActionResult> GetMyInfor(Guid userId) {
            //         // Truy vấn user theo userId
            // }
            // Frontend có thể gửi
            // GET /api/users/infor/123

            // Nhưng nếu người dùng tự thay đổi URL thành: GET /api/users/infor/456

            // Họ có thể thử truy cập thông tin của người khác nếu Backend không kiểm tra quyền truy cập.
            // Thay vào đó, bạn sử dụng: var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Backend tự lấy UserId từ Token đã được xác thực.
            // Như vậy, người dùng không thể chỉ bằng cách thay đổi UserId trên Frontend mà yêu cầu Backend truy vấn thông tin của một tài khoản khác.



            // 5. Luồng hoạt động tổng thể
            // -> Frontend Login (Send Username/Password)
            // -> Backend xác thực (Check account and create JWT containing claims)
            // -> Frontend gửi Request (Attaching a JWT in the Authorization Header)
            // -> Middleware Authentication (JWT Authentication and Constructing the User (ClaimsPrincipal))
            // -> Controller (Get the UserId of the currently logged-in user)
            //  -> Database (Query data by UserId)

// ========================================== OLD ==========================================

            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (userIdClaim == null) {
            //     return Unauthorized(new {
            //         status = 401,
            //         message = "Token không hợp lệ"
            //     });
            // }
// ========================================== OLD ==========================================


            // Có nghĩa là chuyển đổi một chuỗi (string) thành kiểu data Guid
            //Để hiểu rõ, chúng ta đi phân tích từng phần:



            // 1. Guild là gì?
            //Guid (Globally Unique Identifier) là kiểu dữ liệu dùng để tạo ra một mã định danh có độ duy nhất rất cao trên toàn hệ thống.
            // Ví dụ:
            // Văn bản thuần túy: a1b2c3d4-1234-5678-9012-abcdef123456
            // Trong C#, Guid là một kiểu dữ liệu có sẵn, thường được sử dụng làm Primary Key trong Database.
            // Ví dụ Model User của bạn:
            // public class User {
            //     public Guid UserId { get; set; }
            //     public string Username { get; set; }
            //     public string Email { get; set; }
            // }
            // Ở đây, UserId có kiểu dữ liệu Guid, không phải string hay int



            // 2. Tại sao phải sử dụng Guid.Parse()?
            // Quay lại nhìn dòng code phía trên là :var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            // Giá trị trả về của .Value luôn là string.
            // Ví dụ: userIdClaim = "a1b2c3d4-1234-5678-9012-abcdef123456";

            // Nhưng trong Database, UserId lại có kiểu Guid.
            // Vì vậy chúng ta cần chuyển đổi: var userId = Guid.Parse(userIdClaim);

            // Kết quả
            // Trước khi chuyển đổi                        Sau khi chuyển đổi
            // string                                      Guid
            // "a1b2c3d4-..."                              Guid tương ứng


            // Bây giờ bạn có thể sử dụng userId để truy vấn Database: var user = await _context.Users.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            // Nếu không chuyển đổi, bạn sẽ không thể so sánh trực tiếp Guid với string bằng toán tử ==.



            // 3. Guid.Parse() hoạt động như thế nào?
            // Ví dụ:
            // string userIdClaim = "a1b2c3d4-1234-5678-9012-abcdef123456";
            // Guid userId = Guid.Parse(userIdClaim);

            // Nếu chuỗi hợp lệ, việc chuyển đổi thành công.
            // Nhưng nếu dữ liệu không hợp lệ:

            // string userIdClaim = "hello123";
            // Guid userId = Guid.Parse(userIdClaim);


            // C# sẽ phát sinh Exception: System.FormatException
            // Điều này có nghĩa là ứng dụng có thể bị lỗi runtime nếu dữ liệu đầu vào không đúng định dạng.



            // 4. Guid.Parse() và Guid.TryParse() khác nhau thế nào?
            // Đây là điểm chúng ta chú ý khi viết Backend
             
            // Cách 1: Guid.parse()
            // var userId = Guid.Parse(userIdClaim);

            // Chuỗi hợp lệ → Trả về Guid.
            // Chuỗi không hợp lệ → Ném Exception.


            // Cách 2: Guid.TryParse()
            // if (!Guid.TryParse(userIdClaim, out var userId)) {
            //     return Unauthorized(new {
            //         status = 401,
            //         message = "Token không hợp lệ"
            //     });
            // }
            // Chuỗi hợp lệ → Trả về true, đồng thời gán Guid vào userId.
            // Chuỗi không hợp lệ → Trả về false, không phát sinh FormatException.


            //========================= So sánh nhanh =========================
            // So sánh nhanh                        Parse                       TryParse
            // String hợp lệ                        Thành công                  Thành công
            // String ai                            Exception                   Trả về false
            // Phù hợp                              Data đã đảm bảo hợp lệ      Data cần check
            // Khuyến nghị: Vs data lấy từ JWT Claims, nên dùng TryParse() tránh error runtime



        // Sửa
        // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //     if (userIdClaim == null) {
        //         return Unauthorized(new {
        //             status = 401,
        //             message = "Token không hợp lệ"
        //         });
        //     }
        // Thành

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId)) {
                return Unauthorized(new {
                    status = 401,
                    message = "Token không hợp lệ"
                });
            }

// ========================================== OLD ==========================================
            // var userId = Guid.Parse(userIdClaim);
// ========================================== OLD ==========================================


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
                    message = "Không tìm thấy user"
                });
            }

            return Ok(new {
                status = 200,
                data = user
            });
        }

}




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
//     private readonly IAuthService _authService;

//     public AuthController(AppDbContext context, IJwtService jwtService, IAuthService authService) {
//         _context = context;
//         _jwtService = jwtService;
//         _authService = authService;
//     }

// // =========================
// // REGISTER API
// // =========================
//     // [HttpPost("register")]
//     // public async Task<IActionResult> Register(RegisterRequestDto dto) {
//     //     try {
//     //         var result = await _authService.RegisterAsync(dto);

//     //         return StatusCode(StatusCodes.Status201Created, new {
//     //             status = 201,
//     //             message = "Registration successful",
//     //             data = result
//     //         });
//     //     } catch (Exception ex) {
//     //         return BadRequest(new {
//     //             status = 400,
//     //             message = ex.Message
//     //         });
//     //     }
//     // }


// // =========================
// // LOGIN API
// // =========================
//     [HttpPost("login")]
//     public async Task<IActionResult> Login(LoginRequestDto dto) {
//         try {
//             var result = await _authService.LoginAsync(dto);

//             return Ok(new {
//                 status = 200,
//                 message = "Login successful",
//                 data = result
//             });
//         } catch (Exception ex) {
//             return Unauthorized(new {
//                 status = 401,
//                 message = ex.Message
//             });
//         }
//     }



// // =========================
// // GET ME (require JWT)
// // =========================
//     [Authorize]
//     [HttpGet("infor")]
//     public async Task<IActionResult> GetMyInfor() {
//         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//         if (userIdClaim == null) {
//             return Unauthorized(new {
//                 status = 401,
//                 message = "Invalid token"
//             });
//         }

//         var userId = Guid.Parse(userIdClaim);

//         var user = await _context.Users
//             .Where(x => x.UserId == userId)
//             .Select(x => new {
//                 x.UserId,
//                 x.Username,
//                 x.Email,
//                 x.FullName,
//                 x.PhoneNumber,
//                 x.AvatarUrl,
//                 x.CoverPhotoUrl,
//                 x.Bio,
//                 x.Location,
//                 x.DateOfBirth,
//                 x.CreatedAt
//             })
//             .FirstOrDefaultAsync();

//         if (user == null) {
//             return NotFound(new {
//                 status = 404,
//                 message = "User cannot be found"
//             });
//         }

//         return Ok(new {
//             status = 200,
//             data = user
//         });
//     }

// }