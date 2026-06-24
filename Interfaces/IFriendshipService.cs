using whobe.DTOs.Common;
using whobe.DTOs.User;
using whobe.DTOs.User; 
using whobe.Enums;

namespace whobe.Interfaces;

public interface IFriendshipService {
    Task SendRequestAsync(Guid requesterId, Guid receiverId);
    // long vì FriendshipId trong model là long
    Task AcceptRequestAsync(long friendshipId, Guid userId);
    Task DeclineRequestAsync(long friendshipId, Guid userId);
    Task UnfriendAsync(Guid userId, Guid friendId);
    Task CancelRequestAsync(Guid requesterId, Guid receiverId);
    Task BlockAsync(Guid blockerId, Guid blockedId);
    Task UnblockAsync(Guid blockerId, Guid blockedId);
    Task FollowAsync(Guid followerId, Guid followingId);
    Task UnfollowAsync(Guid followerId, Guid followingId);
    Task<FriendshipStatus?> GetStatusAsync(Guid userAId, Guid userBId);
    Task<PagedResultDto<PendingRequestDto>> GetPendingRequestsAsync(Guid userId);   // ← đổi dòng này
    Task<IEnumerable<UserProfileDto>> GetSuggestedFriendsAsync(Guid userId, int count = 10);
    Task<IEnumerable<UserProfileDto>> GetFriendsAsync(Guid userId);
}