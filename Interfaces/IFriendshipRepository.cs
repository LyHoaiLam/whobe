using whobe.Enums;
using whobe.Models;
namespace whobe.Interfaces;

public interface IFriendshipRepository : IGenericRepository<Friendship> {
    Task<Friendship?> GetAsync(Guid userAId, Guid userBId);
    Task<FriendshipStatus?> GetStatusAsync(Guid userAId, Guid userBId);
    Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId);
    Task<IEnumerable<Friendship>> GetFriendListAsync(Guid userId);
    Task<bool> AreFriendsAsync(Guid userAId, Guid userBId);
    Task<int> CountFriendsAsync(Guid userId);
    Task<IEnumerable<User>> GetMutualFriendsAsync(Guid userAId, Guid userBId);
}
