
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using whobe.DTOs.Message;
using whobe.Interfaces;

namespace whobe.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase {
    private readonly IMessageService _messageService;

    private Guid Me =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public MessagesController(IMessageService messageService) {
        _messageService = messageService;
    }


    // GET /api/messages/conversations
    // Lấy danh sách conversations của user đang login
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations() {
        var result = await _messageService.GetUserConversationsAsync(Me);

        return Ok(new {
            status = 200,
            data = result
        });
    }


    // POST /api/messages/conversations/direct/{targetUserId}
    // Mở chat 1-1, tạo conversation nếu chưa tồn tại
    [HttpPost("conversations/direct/{targetUserId:guid}")]
    public async Task<IActionResult> OpenDirectChat(Guid targetUserId) {
        var result = await _messageService.GetOrCreateDirectAsync(
            Me,
            targetUserId
        );

        return Ok(new {
            status = 200,
            data = result
        });
    }


    // GET /api/messages/conversations/{conversationId}
    // Lấy messages trong conversation
    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) {
        var result = await _messageService.GetConversationAsync(
            conversationId,
            Me,
            page,
            pageSize
        );

        return Ok(new {
            status = 200,
            data = result
        });
    }


    [HttpPost("conversations/{conversationId:guid}")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromForm] SendMessageDto dto) {
        dto.ConversationId = conversationId;

        Console.WriteLine($"ConversationId: {dto.ConversationId}");
        Console.WriteLine($"Content: {dto.Content}");
        Console.WriteLine($"MessageType: {dto.MessageType}");

        var result = await _messageService.SendAsync(
            Me,
            dto
        );

        return Ok(new {
            status = 200,
            data = result
        });
    }


    // PUT /api/messages/conversations/{conversationId}/read
    // Đánh dấu conversation đã đọc
    [HttpPut("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId) {
        await _messageService.MarkAsReadAsync(
            conversationId,
            Me
        );

        return Ok(new {
            status = 200,
            data = new {
                message = "Đã đánh dấu đã đọc."
            }
        });
    }


    // GET /api/messages/unread-count
    // Tổng số message chưa đọc
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount() {
        var result = await _messageService.GetUnreadCountAsync(Me);

        return Ok(new {
            status = 200,
            data = result
        });
    }


    // DELETE /api/messages/{messageId}
    // Xóa message của chính mình
    [HttpDelete("{messageId:long}")]
    public async Task<IActionResult> DeleteMessage(long messageId) {
        await _messageService.DeleteMessageAsync(
            messageId,
            Me
        );

        return Ok(new {
            status = 200,
            data = new {
                message = "Đã xóa tin nhắn."
            }
        });
    }


    // PUT /api/messages/{messageId}
    // Chỉnh sửa message của chính mình
    [HttpPut("{messageId:long}")]
    public async Task<IActionResult> EditMessage(long messageId, [FromBody] EditMessageRequest request) {
        var result = await _messageService.EditMessageAsync(
            messageId,
            Me,
            request.NewContent
        );

        return Ok(new {
            status = 200,
            data = result
        });
    }


    // POST /api/messages/conversations/group
    // Tạo group conversation
    [HttpPost("conversations/group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupConversationDto dto) {
        var result = await _messageService.CreateGroupConversationAsync(
            Me,
            dto
        );

        return Ok(new {
            status = 200,
            data = result
        });
    }
}


// Request DTO cho API edit message
public class EditMessageRequest {
    public string NewContent { get; set; } = string.Empty;
}

