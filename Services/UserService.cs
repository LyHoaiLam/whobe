using whobe.DTOs.Common;
using whobe.DTOs.User;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;
//profile, update avatar/cover, search, deactivate
public class UserService : IUserService {
    private readonly IUserRepository _userRepo;
    private readonly IFriendshipRepository _friendRepo;
    private readonly IFileStorageService _storage;

    public UserService(IUserRepository userRepo, IFriendshipRepository friendRepo, IFileStorageService storage) {
        _userRepo = userRepo;
        _friendRepo = friendRepo;
        _storage = storage;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, Guid currentUserId) {
        var user = await _userRepo.GetProfileAsync(userId);
        if (user == null) return null;

        var friends = await _userRepo.GetFriendsAsync(userId);
        var followers = await _userRepo.GetFollowersAsync(userId);
        var following = await _userRepo.GetFollowingAsync(userId);
        var status = await _friendRepo.GetStatusAsync(currentUserId, userId);
        var isFollowing = followers.Any(f => f.UserId == currentUserId);

        return MapToDto(user, friends.Count(), followers.Count(), following.Count(),
            status?.ToString(), isFollowing);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto) {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Người dùng không tồn tại.");

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Bio != null) user.Bio = dto.Bio;
        if (dto.Website != null) user.Website = dto.Website;
        if (dto.Location != null) user.Location = dto.Location;
        if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender.HasValue) user.Gender = (Enums.UserGender)dto.Gender.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.SaveChangesAsync();
        return MapToDto(user, 0, 0, 0, null, false);
    }

    public async Task<bool> UpdateAvatarAsync(Guid userId, IFormFile file) {
        if (!_storage.IsValidImage(file))
            throw new Exception("File ảnh không hợp lệ (chỉ chấp nhận jpg/png/webp, tối đa 10MB).");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Người dùng không tồn tại.");

        if (user.AvatarUrl != null)
            await _storage.DeleteAsync(user.AvatarUrl);

        user.AvatarUrl = await _storage.UploadAsync(file, "avatars");
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCoverPhotoAsync(Guid userId, IFormFile file) {
        if (!_storage.IsValidImage(file))
            throw new Exception("File ảnh không hợp lệ.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Người dùng không tồn tại.");

        if (user.CoverPhotoUrl != null)
            await _storage.DeleteAsync(user.CoverPhotoUrl);

        user.CoverPhotoUrl = await _storage.UploadAsync(file, "covers");
        user.UpdatedAt     = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResultDto<UserProfileDto>> SearchUsersAsync(string keyword, int page, int pageSize) {
        var users = await _userRepo.SearchUsersAsync(keyword, page, pageSize);
        var items = users.Select(u => MapToDto(u, 0, 0, 0, null, false));
        
        return new PagedResultDto<UserProfileDto> {
            Items = items,
            TotalCount = items.Count(),
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<IEnumerable<UserProfileDto>> GetFriendsAsync(Guid userId) {
        var users = await _userRepo.GetFriendsAsync(userId);
        return users.Select(u => MapToDto(u, 0, 0, 0, "Accepted", false));
    }

    public async Task<IEnumerable<UserProfileDto>> GetFollowersAsync(Guid userId) {
        var users = await _userRepo.GetFollowersAsync(userId);
        return users.Select(u => MapToDto(u, 0, 0, 0, null, false));
    }

    public async Task<IEnumerable<UserProfileDto>> GetFollowingAsync(Guid userId) {
        var users = await _userRepo.GetFollowingAsync(userId);
        return users.Select(u => MapToDto(u, 0, 0, 0, null, true));
    }

    public async Task<IEnumerable<UserProfileDto>> GetMutualFriendsAsync(Guid userAId, Guid userBId) {
        var users = await _friendRepo.GetMutualFriendsAsync(userAId, userBId);
        return users.Select(u => MapToDto(u, 0, 0, 0, "Accepted", false));
    }

    public async Task DeactivateAccountAsync(Guid userId) {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Người dùng không tồn tại.");
        user.IsActive  = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.SaveChangesAsync();
    }

    // ── Helper ───────────────────────────────────────────
    private static UserProfileDto MapToDto(User u, int friends, int followers, int following, string? status, bool isFollowing) => new() {
        UserId = u.UserId,
        Username = u.Username,
        FullName = u.FullName,
        AvatarUrl = u.AvatarUrl,
        CoverPhotoUrl = u.CoverPhotoUrl,
        Bio = u.Bio,
        Website = u.Website,
        Location = u.Location,
        IsVerified = u.IsVerified,
        FriendCount = friends,
        FollowerCount = followers,
        FollowingCount = following,
        FriendshipStatus = status,
        IsFollowing = isFollowing,
        CreatedAt = u.CreatedAt,
    };
}
