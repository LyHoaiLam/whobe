using whobe.Models;
namespace whobe.Interfaces;

public interface IMessageRepository : IGenericRepository<Message> {
    Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, int page, int pageSize);
    Task<Message?> GetWithDetailsAsync(long messageId);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
    Task<Conversation?> GetDirectConversationAsync(Guid userAId, Guid userBId);
    Task<Conversation?> GetConversationWithMembersAsync(Guid conversationId);
    Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId);
    Task<int> CountUnreadAsync(Guid userId);
}
