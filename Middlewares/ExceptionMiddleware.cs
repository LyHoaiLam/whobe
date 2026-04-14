// using System.Net;
// using System.Text.Json;

// namespace whobe.Middlewares;

// public class ExceptionMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<ExceptionMiddleware> _logger;

//     public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger) {
//         _next = next;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContext context) {
//         try {
//             await _next(context);
//         } catch (Exception ex) {
//             _logger.LogError(ex, ex.Message);
//             await HandleExceptionAsync(context, ex);
//         }
//     }

//     private static async Task HandleExceptionAsync(HttpContext context, Exception ex) {
//         context.Response.ContentType = "application/json";

//         // Exception thường (nghiệp vụ) => 400
//         // Exception khác => 500
//         var isBusinessError = ex is Exception
//             && ex.GetType() == typeof(Exception);

//         var statusCode = isBusinessError
//             ? (int)HttpStatusCode.BadRequest
//             : (int)HttpStatusCode.InternalServerError;

//         var message = isBusinessError
//             ? ex.Message                          // "Không thể gửi lời mời cho chính mình."
//             : "Đã xảy ra lỗi, vui lòng thử lại."; // ẩn lỗi hệ thống với client

//         context.Response.StatusCode = statusCode;

//         var response = new {
//             status = statusCode,
//             data   = new { message }
//         };

//         var json = JsonSerializer.Serialize(response, new JsonSerializerOptions {
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//         });

//         await context.Response.WriteAsync(json);
//     }
// }
