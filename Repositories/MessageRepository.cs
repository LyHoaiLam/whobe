using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository {
    public MessageRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(
        Guid conversationId, int page, int pageSize) =>
        await _set
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .Include(m => m.Sender)
            .Include(m => m.ReplyTo)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<Message?> GetWithDetailsAsync(long messageId) =>
        await _set
            .Include(m => m.Sender)
            .Include(m => m.ReplyTo)
            .Include(m => m.ReadReceipts)
            .FirstOrDefaultAsync(m => m.MessageId == messageId);

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId) =>
        await _db.Conversations
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.LastMessage).ThenInclude(m => m!.Sender)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

    public async Task<Conversation?> GetDirectConversationAsync(Guid userAId, Guid userBId) =>
        await _db.Conversations
            .Where(c => c.Type == 0 &&
                c.Members.Any(m => m.UserId == userAId) &&
                c.Members.Any(m => m.UserId == userBId))
            .Include(c => c.Members)
            .FirstOrDefaultAsync();

    public async Task<Conversation?> GetConversationWithMembersAsync(Guid conversationId) =>
        await _db.Conversations
            .Include(c => c.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

    public async Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId) {
        var member = await _db.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId);
        if (member != null) {
            member.LastReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> CountUnreadAsync(Guid userId) {
        var memberships = await _db.ConversationMembers
            .Where(m => m.UserId == userId)
            .ToListAsync();

        int total = 0;
        foreach (var m in memberships) {
            var count = await _set.CountAsync(msg =>
                msg.ConversationId == m.ConversationId &&
                msg.SenderId != userId &&
                !msg.IsDeleted &&
                msg.SentAt > (m.LastReadAt ?? DateTime.MinValue));
            total += count;
        }
        return total;
    }

    public async Task<Conversation?> GetDirectConversationByKeyAsync(
    string directKey) {
        return await _db.Conversations
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x =>
                x.Type == 0 &&
                x.DirectKey == directKey
            );
    }


}
