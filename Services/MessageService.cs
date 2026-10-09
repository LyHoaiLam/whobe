using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.DTOs.Common;
using whobe.DTOs.Message;
using whobe.Interfaces;
using whobe.Models;
using Microsoft.AspNetCore.SignalR;
using whobe.Hubs;
namespace whobe.Services;
public class MessageService : IMessageService {
    private readonly IMessageRepository  _msgRepo;
    private readonly IFileStorageService _storage;
    private readonly INotificationService _notifService;
    private readonly IHubContext<ChatHub> _chatHub; //chat realtime
    private readonly AppDbContext _db;

    public MessageService(IMessageRepository msgRepo, IFileStorageService storage, INotificationService notifService, AppDbContext db, IHubContext<ChatHub> chatHub) {
        _msgRepo = msgRepo;
        _storage = storage;
        _notifService = notifService;
        _db = db;
        _chatHub = chatHub;
    }


    private static string CreateDirectKey(Guid userAId, Guid userBId) {
        var first = userAId.CompareTo(userBId) < 0
            ? userAId
            : userBId;

        var second = userAId.CompareTo(userBId) < 0
            ? userBId
            : userAId;

        return $"{first}:{second}";
    }


    public async Task<MessageResponseDto> SendAsync(Guid senderId, SendMessageDto dto) {
        // Không có content thì không gửi
        if (string.IsNullOrWhiteSpace(dto.Content)) {
            throw new Exception("Nội dung tin nhắn không được để trống.");
        }

        var conv = await _msgRepo.GetConversationWithMembersAsync(dto.ConversationId)
            ?? throw new Exception("Cuộc hội thoại không tồn tại.");

        if (!conv.Members.Any(m => m.UserId == senderId))
            throw new Exception("Bạn không phải thành viên của cuộc hội thoại này.");

        var message = new Message {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            ReplyToId = dto.ReplyToId,
            MessageType = (Enums.MessageType)dto.MessageType,
            Content = dto.Content,
        };

        await _db.Messages.AddAsync(message);

        conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        conv.LastMessageId = message.MessageId;
        await _db.SaveChangesAsync();

        foreach (var member in conv.Members.Where(m => m.UserId != senderId)) {
            await _notifService.CreateAsync(
                member.UserId,
                senderId,
                Enums.NotificationType.Message,
                3,
                dto.ConversationId
            );
        }

        var full = await _msgRepo.GetWithDetailsAsync(message.MessageId);

        var response = MapToDto(full!, false);

        await _chatHub.Clients.Group($"conv_{dto.ConversationId}").SendAsync("ReceiveMessage", response);

        return response;
    }



    // ====================================== Conversation BY "conversationId" ======================================
    public async Task<PagedResultDto<MessageResponseDto>> GetConversationAsync(Guid conversationId, Guid userId, int page, int pageSize) {
        var conv = await _msgRepo.GetConversationWithMembersAsync(conversationId)
            ?? throw new Exception("Cuộc hội thoại không tồn tại.");

        if (!conv.Members.Any(m => m.UserId == userId))
            throw new Exception("Bạn không có quyền xem cuộc hội thoại này.");

        var messages = await _msgRepo.GetConversationMessagesAsync(conversationId, page, pageSize);
        var member = conv.Members.First(m => m.UserId == userId);

        return new PagedResultDto<MessageResponseDto> {
            Items = messages.Select(m => MapToDto(m, m.SentAt > (member.LastReadAt ?? DateTime.MinValue))),
            TotalCount = messages.Count(),
            Page = page,
            PageSize = pageSize,
        };
    }
    // ========================================================================


    public async Task<IEnumerable<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId) {
        var convs = await _msgRepo.GetUserConversationsAsync(userId);
        var result = new List<ConversationSummaryDto>();

        foreach (var conv in convs) {
            var member = conv.Members.First(m => m.UserId == userId);
            var unread = await _db.Messages.CountAsync(m =>
                m.ConversationId == conv.ConversationId &&
                m.SenderId != userId && !m.IsDeleted &&
                m.SentAt > (member.LastReadAt ?? DateTime.MinValue));

            // Với chat 1-1: lấy tên + avatar của người kia
            string? name = conv.Name;
            string? avatar = conv.AvatarUrl;
            if (conv.Type == 0) {
                var other = conv.Members.FirstOrDefault(m => m.UserId != userId)?.User;
                name = other?.FullName;
                avatar = other?.AvatarUrl;
            }

            result.Add(new ConversationSummaryDto {
                ConversationId = conv.ConversationId,
                Type = conv.Type,
                Name = name,
                AvatarUrl = avatar,
                LastMessage = conv.LastMessage?.Content,
                LastMessageAt = conv.LastMessage?.SentAt,
                UnreadCount = unread,
                Members = conv.Members.Select(m => new MemberSnapshotDto {
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? "",
                    AvatarUrl = m.User?.AvatarUrl,
                }).ToList(),
            });
        }
        return result;
    }


    // public async Task<ConversationSummaryDto> GetOrCreateDirectAsync(Guid userAId, Guid userBId) {
    //     var existing = await _msgRepo.GetDirectConversationAsync(userAId, userBId);
    //     if (existing != null)
    //         return await ToSummary(existing, userAId);

    //     var conv = new Conversation { Type = 0, CreatedBy = userAId };
    //     _db.Conversations.Add(conv);
    //     await _db.SaveChangesAsync();

    //     _db.ConversationMembers.AddRange(
    //         new ConversationMember { ConversationId = conv.ConversationId, UserId = userAId },
    //         new ConversationMember { ConversationId = conv.ConversationId, UserId = userBId }
    //     );
    //     await _db.SaveChangesAsync();

    //     var full = await _msgRepo.GetConversationWithMembersAsync(conv.ConversationId);
    //     return await ToSummary(full!, userAId);
    // }
    
    

public async Task<ConversationSummaryDto> GetOrCreateDirectAsync(
    Guid userAId,
    Guid userBId)
{


    var directKey = CreateDirectKey(userAId, userBId);

var existing = await _msgRepo.GetDirectConversationByKeyAsync(
    directKey
);

if (existing != null)
    return await ToSummary(existing, userAId);

    if (existing != null)
        return await ToSummary(existing, userAId);

    await using var transaction =
        await _db.Database.BeginTransactionAsync();

    try
    {
        var conv = new Conversation
        {
            Type = 0,
            CreatedBy = userAId,
            DirectKey = directKey
        };

        _db.Conversations.Add(conv);

        await _db.SaveChangesAsync();

        _db.ConversationMembers.AddRange(
            new ConversationMember
            {
                ConversationId = conv.ConversationId,
                UserId = userAId
            },
            new ConversationMember
            {
                ConversationId = conv.ConversationId,
                UserId = userBId
            }
        );

        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        var full =
            await _msgRepo.GetConversationWithMembersAsync(
                conv.ConversationId
            );

        return await ToSummary(full!, userAId);
    }
    catch (DbUpdateException)
    {
        await transaction.RollbackAsync();

        var existingAfterRace =
            await _msgRepo.GetDirectConversationByKeyAsync(
                directKey
            );

        if (existingAfterRace != null)
            return await ToSummary(existingAfterRace, userAId);

        throw;
    }
}
 

    public async Task<ConversationSummaryDto> CreateGroupConversationAsync(Guid creatorId, CreateGroupConversationDto dto) {
        var conv = new Conversation {
            Type = 1,
            Name = dto.Name,
            CreatedBy = creatorId,
        };
        _db.Conversations.Add(conv);
        await _db.SaveChangesAsync();

        var memberIds = dto.MemberIds.Distinct().ToList();
        if (!memberIds.Contains(creatorId)) memberIds.Add(creatorId);

        _db.ConversationMembers.AddRange(memberIds.Select(id =>
            new ConversationMember
            {
                ConversationId = conv.ConversationId,
                UserId = id,
                Role = (byte)(id == creatorId ? 2 : 0), // 2=Owner
            }));
        await _db.SaveChangesAsync();

        var full = await _msgRepo.GetConversationWithMembersAsync(conv.ConversationId);
        return await ToSummary(full!, creatorId);
    }


    public async Task MarkAsReadAsync(Guid conversationId, Guid userId) =>
        await _msgRepo.MarkMessagesAsReadAsync(conversationId, userId);

    public async Task<int> GetUnreadCountAsync(Guid userId) => await _msgRepo.CountUnreadAsync(userId);

    public async Task DeleteMessageAsync(long messageId, Guid userId) {
        var msg = await _db.Messages.FindAsync(messageId) ?? throw new Exception("Tin nhắn không tồn tại.");
        if (msg.SenderId != userId)
            throw new Exception("Bạn không có quyền xóa tin nhắn này.");
        msg.IsDeleted = true;
        msg.Content   = null;
        await _db.SaveChangesAsync();
    }


    public async Task<MessageResponseDto> EditMessageAsync(long messageId, Guid userId, string newContent) {
        var msg = await _db.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.MessageId == messageId)
            ?? throw new Exception("Tin nhắn không tồn tại.");

        if (msg.SenderId != userId)
            throw new Exception("Bạn không có quyền chỉnh sửa tin nhắn này.");

        msg.Content = newContent;
        msg.EditedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(msg, false);
    }


    // ── Helpers ──────────────────────────────────────────
    private async Task<ConversationSummaryDto> ToSummary(Conversation conv, Guid userId) {
        var member = conv.Members.FirstOrDefault(m => m.UserId == userId);
        var unread = member == null ? 0 : await _db.Messages.CountAsync(m =>
            m.ConversationId == conv.ConversationId &&
            m.SenderId != userId && !m.IsDeleted &&
            m.SentAt > (member.LastReadAt ?? DateTime.MinValue));

        string? name = conv.Name, avatar = conv.AvatarUrl;
        if (conv.Type == 0) {
            var other = conv.Members.FirstOrDefault(m => m.UserId != userId)?.User;
            name = other?.FullName;
            avatar = other?.AvatarUrl;
        }

        return new ConversationSummaryDto {
            ConversationId = conv.ConversationId,
            Type = conv.Type,
            Name = name,
            AvatarUrl = avatar,
            UnreadCount = unread,
            Members = conv.Members.Select(m => new MemberSnapshotDto {
                UserId = m.UserId,
                FullName = m.User?.FullName ?? "",
                AvatarUrl = m.User?.AvatarUrl,
            }).ToList(),
        };
    }


    private static MessageResponseDto MapToDto(Message m, bool isUnread) => new() {
        MessageId = m.MessageId,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        SenderName = m.Sender?.FullName ?? "",
        SenderAvatar = m.Sender?.AvatarUrl,
        MessageType = (int)m.MessageType,
        Content = m.Content,
        MediaUrl = m.MediaUrl,
        ReplyToId = m.ReplyToId,
        ReplyToContent = m.ReplyTo?.Content,
        IsDeleted = m.IsDeleted,
        IsRead = !isUnread,
        SentAt = m.SentAt,
        EditedAt = m.EditedAt,
    };
}

