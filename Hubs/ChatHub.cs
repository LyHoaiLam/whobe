// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.SignalR;
// using System.Security.Claims;
// namespace whobe.Hubs;
// [Authorize]
// public class ChatHub : Hub {
//     // Khi kết nối, join vào group theo userId
//     // => có thể push tin nhắn đúng người bằng Clients.Group(userId)
//     public override async Task OnConnectedAsync() {
//         var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
//         if (!string.IsNullOrEmpty(userId))
//             await Groups.AddToGroupAsync(Context.ConnectionId, userId);
//         await base.OnConnectedAsync();
//     }
//     public override async Task OnDisconnectedAsync(Exception? exception) {
//         var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
//         if (!string.IsNullOrEmpty(userId))
//             await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
//         await base.OnDisconnectedAsync(exception);
//     }
//     // Client gọi để join vào room của 1 conversation cụ thể
//     public async Task JoinConversation(string conversationId) =>
//         await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
//     // Client gọi để rời khỏi conversation
//     public async Task LeaveConversation(string conversationId) =>
//         await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using whobe.Interfaces;
namespace whobe.Hubs;

[Authorize]
public class ChatHub : Hub {
    private readonly IMessageRepository _msgRepo;

    public ChatHub(IMessageRepository msgRepo) {
        _msgRepo = msgRepo;
    }

    // Khi user kết nối SignalR
    // => join vào group riêng của user
    public override async Task OnConnectedAsync() {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId)) {
            // Join group riêng của user
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                userId
            );

            // Lưu connection của user
            UserConnectionManager.Add(userId, Context.ConnectionId);

            // Báo cho các client khác biết user vừa online
            await Clients.All.SendAsync(
                "UserOnline",
                new {UserId = userId}
            );
        }

        await base.OnConnectedAsync();
    }

    // Khi disconnect
    public override async Task OnDisconnectedAsync(Exception? exception) {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId)) {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                userId
            );

            UserConnectionManager.Remove(userId,Context.ConnectionId);

            // Chỉ báo Offline nếu user
            // không còn connection nào
            if (!UserConnectionManager.IsOnline(userId)) {
                await Clients.All.SendAsync(
                    "UserOffline",
                    new {UserId = userId}
                );
            }
        }

        await base.OnDisconnectedAsync(exception);
    }


    // Join conversation
    public async Task JoinConversation(Guid conversationId) {
        var userIdString = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!Guid.TryParse(userIdString, out var userId))
            throw new HubException("User không hợp lệ.");

        var conversation =
            await _msgRepo.GetConversationWithMembersAsync(
                conversationId
            );

        if (conversation == null)
            throw new HubException("Cuộc hội thoại không tồn tại.");

        var isMember = conversation.Members.Any(m => m.UserId == userId);

        if (!isMember)
            throw new HubException("Bạn không phải thành viên của cuộc hội thoại này.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"conv_{conversationId}"
        );
    }


    public async Task Typing(string conversationId, bool isTyping) {

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.GroupExcept(
            $"conv_{conversationId}",
            Context.ConnectionId
        ).SendAsync("UserTyping", new {
            UserId = userId,
            UserName = userName,
            IsTyping = isTyping
        });

    }
    // .GroupExcept($"conv_{conversationId}", Context.ConnectionId ===> Nghĩa là: Gửi sự kiện typing cho những người khác trong conversation, không gửi lại cho chính người đang gõ. Nếu không dùng GroupExcept, User A cũng sẽ nhận event của chính mình.

    // Leave conversation
    public async Task LeaveConversation(Guid conversationId) {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"conv_{conversationId}"
        );
    }
    

    public Task<string[]> GetOnlineUsers() {
        return Task.FromResult(UserConnectionManager.GetOnlineUsers());
    }
}