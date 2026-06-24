using whobe.DTOs.Common;
using whobe.DTOs.Message;

namespace whobe.Interfaces;

public interface IMessageService {
    Task<MessageResponseDto> SendAsync(Guid senderId, SendMessageDto dto);
    Task<PagedResultDto<MessageResponseDto>> GetConversationAsync(Guid conversationId, Guid userId, int page, int pageSize);
    Task<IEnumerable<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId);
    Task<ConversationSummaryDto> GetOrCreateDirectAsync(Guid userAId, Guid userBId);
    Task<ConversationSummaryDto> CreateGroupConversationAsync(Guid creatorId, CreateGroupConversationDto dto);
    Task MarkAsReadAsync(Guid conversationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task DeleteMessageAsync(long messageId, Guid userId);
    Task<MessageResponseDto> EditMessageAsync(long messageId, Guid userId, string newContent);
}
