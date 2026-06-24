using whobe.DTOs.Common;
using whobe.DTOs.User;
namespace whobe.Interfaces;

public interface IUserService {
    Task<UserProfileDto?> GetProfileAsync(Guid userId, Guid currentUserId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<bool> UpdateAvatarAsync(Guid userId, IFormFile file);
    Task<bool> UpdateCoverPhotoAsync(Guid userId, IFormFile file);
    Task<PagedResultDto<UserProfileDto>> SearchUsersAsync(string keyword, int page, int pageSize);
    Task<IEnumerable<UserProfileDto>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<UserProfileDto>> GetFollowersAsync(Guid userId);
    Task<IEnumerable<UserProfileDto>> GetFollowingAsync(Guid userId);
    Task<IEnumerable<UserProfileDto>> GetMutualFriendsAsync(Guid userAId, Guid userBId);
    Task DeactivateAccountAsync(Guid userId);
}
