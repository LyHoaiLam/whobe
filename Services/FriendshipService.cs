using Microsoft.EntityFrameworkCore;
using whobe.Data;
using whobe.DTOs.Common;
using whobe.DTOs.User;
using whobe.Enums;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;

public class FriendshipService : IFriendshipService {
    private readonly IFriendshipRepository _friendRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notifService;
    private readonly AppDbContext _db;

    public FriendshipService(
        IFriendshipRepository friendRepo,
        IUserRepository userRepo,
        INotificationService notifService,
        AppDbContext db) {
        _friendRepo = friendRepo;
        _userRepo = userRepo;
        _notifService = notifService;
        _db = db;
    }


    // ── Lấy danh sách bạn ──────────────────────────────
public async Task<IEnumerable<UserProfileDto>> GetFriendsAsync(Guid userId) {
    // Lấy những friendship mà mình là Requester
    var requesterSide = await _db.Friendships
        .Where(friend => friend.Status == FriendshipStatus.Accepted && friend.RequesterId == userId)
        .Select(friend => friend.Receiver)
        .Where(user => user.IsActive)
        .ToListAsync();

    // Lấy những friendship mà mình là Receiver
    var receiverSide = await _db.Friendships
        .Where(friend => friend.Status == FriendshipStatus.Accepted && friend.ReceiverId == userId)
        .Select(friend => friend.Requester)
        .Where(user => user.IsActive)
        .ToListAsync();

    // Gộp 2 danh sách lại
    var friends = requesterSide.Concat(receiverSide);

    return friends.Select(user => new UserProfileDto {
        UserId = user.UserId,
        Username = user.Username,
        FullName = user.FullName,
        AvatarUrl = user.AvatarUrl,
        CreatedAt = user.CreatedAt
    });
}




    // ── Gửi lời mời kết bạn ──────────────────────────────
    public async Task SendRequestAsync(Guid requesterId, Guid receiverId) {
        if (requesterId == receiverId)
            throw new Exception("Không thể gửi lời mời cho chính mình.");

        var existing = await _friendRepo.GetAsync(requesterId, receiverId);
        if (existing != null) {
            if (existing.Status == FriendshipStatus.Accepted)
                throw new Exception("Hai người đã là bạn bè.");

            if (existing.Status == FriendshipStatus.Pending)
                throw new Exception("Lời mời kết bạn đang chờ xác nhận.");

            if (existing.Status == FriendshipStatus.Declined) {
                // Cho phép gửi lại nếu đã bị từ chối
                existing.Status = FriendshipStatus.Pending;
                existing.CreatedAt = DateTime.UtcNow;
                existing.RespondedAt = null;
                await _friendRepo.SaveChangesAsync();
                await PushFriendRequestNotificationAsync(requesterId, receiverId);
                return;
            }
        }

        var blocked = await _db.BlockLists.AnyAsync(b =>
            (b.BlockerId == requesterId && b.BlockedId == receiverId) ||
            (b.BlockerId == receiverId && b.BlockedId == requesterId));

        if (blocked)
            throw new Exception("Không thể gửi lời mời kết bạn.");

        var friendship = new Friendship {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = FriendshipStatus.Pending,
        };

        await _friendRepo.AddAsync(friendship);
        await _friendRepo.SaveChangesAsync();

        await PushFriendRequestNotificationAsync(requesterId, receiverId);
    }

    // ── Chấp nhận lời mời ────────────────────────────────
    // FriendshipId là long (khớp với model Friendship.FriendshipId)
    public async Task AcceptRequestAsync(long friendshipId, Guid userId) {
        var friendship = await _db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)   // Include Receiver để lấy FullName
            .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId)
            ?? throw new Exception("Lời mời kết bạn không tồn tại.");

        if (friendship.ReceiverId != userId)
            throw new Exception("Bạn không có quyền thực hiện.");

        if (friendship.Status != FriendshipStatus.Pending)
            throw new Exception("Lời mời không hợp lệ.");

        friendship.Status = FriendshipStatus.Accepted;
        friendship.RespondedAt = DateTime.UtcNow;
        await _friendRepo.SaveChangesAsync();

        // Push thông báo cho người gửi lời mời
        await _notifService.CreateAsync(
            recipientId: friendship.RequesterId,
            actorId: userId,
            type: NotificationType.FriendAccepted,
            entityType: 2,
            entityId: userId,
            message: $"{friendship.Receiver?.FullName ?? "Ai đó"} đã chấp nhận lời mời kết bạn của bạn."
        );
    }

    // ── Từ chối lời mời ──────────────────────────────────
    // FriendshipId là long (khớp với model)
    public async Task DeclineRequestAsync(long friendshipId, Guid userId) {
        var friendship = await _db.Friendships
            .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId)
            ?? throw new Exception("Lời mời kết bạn không tồn tại.");

        if (friendship.ReceiverId != userId)
            throw new Exception("Bạn không có quyền thực hiện.");

        if (friendship.Status != FriendshipStatus.Pending)
            throw new Exception("Lời mời không hợp lệ.");

        friendship.Status = FriendshipStatus.Declined;
        friendship.RespondedAt = DateTime.UtcNow;
        await _friendRepo.SaveChangesAsync();
    }

    // ── Hủy kết bạn ──────────────────────────────────────
    public async Task UnfriendAsync(Guid userId, Guid friendId) {
        var friendship = await _friendRepo.GetAsync(userId, friendId)
            ?? throw new Exception("Không tìm thấy quan hệ bạn bè.");

        await _friendRepo.DeleteAsync(friendship);
        await _friendRepo.SaveChangesAsync();
    }

    // ── Thu hồi lời mời đang chờ ─────────────────────────
    public async Task CancelRequestAsync(Guid requesterId, Guid receiverId) {
        var friendship = await _db.Friendships
            .FirstOrDefaultAsync(f =>
                f.RequesterId == requesterId &&
                f.ReceiverId == receiverId  &&
                f.Status == FriendshipStatus.Pending)
            ?? throw new Exception("Không tìm thấy lời mời kết bạn đang chờ.");

        _db.Friendships.Remove(friendship);
        await _db.SaveChangesAsync();
    }

    // ── Block / Unblock ───────────────────────────────────
    public async Task BlockAsync(Guid blockerId, Guid blockedId) {
        // Hủy quan hệ bạn bè nếu có
        var friendship = await _friendRepo.GetAsync(blockerId, blockedId);
        if (friendship != null) {
            await _friendRepo.DeleteAsync(friendship);
            await _friendRepo.SaveChangesAsync();
        }

        var already = await _db.BlockLists.AnyAsync(b =>
            b.BlockerId == blockerId && b.BlockedId == blockedId);
        if (already) return;

        _db.BlockLists.Add(new BlockList {
            BlockerId = blockerId,
            BlockedId = blockedId,
        });
        await _db.SaveChangesAsync();
    }

    public async Task UnblockAsync(Guid blockerId, Guid blockedId) {
        var block = await _db.BlockLists
            .FirstOrDefaultAsync(b =>
                b.BlockerId == blockerId && b.BlockedId == blockedId);

        if (block != null) {
            _db.BlockLists.Remove(block);
            await _db.SaveChangesAsync();
        }
    }

    // ── Follow / Unfollow ─────────────────────────────────
    public async Task FollowAsync(Guid followerId, Guid followingId) {
        if (followerId == followingId) return;

        var already = await _db.Follows.AnyAsync(f =>
            f.FollowerId == followerId && f.FollowingId == followingId);
        if (already) return;

        _db.Follows.Add(new Follow {
            FollowerId = followerId,
            FollowingId = followingId,
        });
        await _db.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingId) {
        var follow = await _db.Follows
            .FirstOrDefaultAsync(f =>
                f.FollowerId == followerId && f.FollowingId == followingId);

        if (follow != null) {
            _db.Follows.Remove(follow);
            await _db.SaveChangesAsync();
        }
    }

    // ── Queries ───────────────────────────────────────────
    public async Task<FriendshipStatus?> GetStatusAsync(Guid userAId, Guid userBId) =>
        await _friendRepo.GetStatusAsync(userAId, userBId);

 public async Task<PagedResultDto<PendingRequestDto>> GetPendingRequestsAsync(Guid userId) {
    var list = await _friendRepo.GetPendingRequestsAsync(userId);
    return new PagedResultDto<PendingRequestDto> {
        Items = list.Select(f => new PendingRequestDto {
            FriendshipId = f.FriendshipId,
            UserId = f.Requester.UserId,
            Username = f.Requester.Username,
            FullName = f.Requester.FullName,
            AvatarUrl = f.Requester.AvatarUrl,
            CreatedAt = f.CreatedAt,
        }),
        TotalCount = list.Count(),
        Page = 1,
        PageSize = 100,
    };
}


    public async Task<IEnumerable<UserProfileDto>> GetSuggestedFriendsAsync(Guid userId, int count = 10) {
        // Lấy danh sách bạn hiện tại + bản thân
        var friendIds = await _db.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                (f.RequesterId == userId || f.ReceiverId == userId))
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        friendIds.Add(userId);

        // Bạn của bạn chưa kết bạn với mình
        var suggestions = await _db.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                (friendIds.Contains(f.RequesterId) || friendIds.Contains(f.ReceiverId)))
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .Where(id => !friendIds.Contains(id))
            .Distinct()
            .Take(count)
            .ToListAsync();

        var users = await _db.Users
            .Where(u => suggestions.Contains(u.UserId) && u.IsActive)
            .ToListAsync();

        return users.Select(u => new UserProfileDto {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            AvatarUrl = u.AvatarUrl,
            CreatedAt = u.CreatedAt,
        });
    }

    // ── Private helper ────────────────────────────────────
    private async Task PushFriendRequestNotificationAsync(Guid requesterId, Guid receiverId) {
        var requester = await _userRepo.GetByIdAsync(requesterId);

        await _notifService.CreateAsync(
            recipientId: receiverId,
            actorId: requesterId,
            type: NotificationType.FriendRequest,
            entityType: 2,
            entityId: requesterId,
            message: $"{requester?.FullName ?? "Ai đó"} đã gửi lời mời kết bạn cho bạn."
        );
    }
}
