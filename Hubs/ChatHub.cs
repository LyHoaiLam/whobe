using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace whobe.Hubs;

[Authorize]
public class ChatHub : Hub {
    // Khi kết nối, join vào group theo userId
    // => có thể push tin nhắn đúng người bằng Clients.Group(userId)
    public override async Task OnConnectedAsync() {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        await base.OnDisconnectedAsync(exception);
    }

    // Client gọi để join vào room của 1 conversation cụ thể
    public async Task JoinConversation(string conversationId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");

    // Client gọi để rời khỏi conversation
    public async Task LeaveConversation(string conversationId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
}
