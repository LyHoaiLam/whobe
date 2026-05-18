using whobe.DTOs.Common;
using whobe.DTOs.Page;
using whobe.DTOs.User;
namespace whobe.Interfaces;

public interface IPageService {
    Task<PageResponseDto> CreateAsync(Guid userId, CreatePageDto dto);
    Task<PageResponseDto?> GetByIdAsync(Guid pageId, Guid currentUserId);
    Task<PageResponseDto?> GetByUsernameAsync(string username, Guid currentUserId);
    Task<PageResponseDto> UpdateAsync(Guid pageId, Guid userId, UpdatePageDto dto);
    Task DeleteAsync(Guid pageId, Guid userId);
    Task FollowAsync(Guid pageId, Guid userId);
    Task UnfollowAsync(Guid pageId, Guid userId);
    Task<PagedResultDto<UserProfileDto>> GetFollowersAsync(Guid pageId, int page, int pageSize);
    Task<PagedResultDto<PageResponseDto>> GetUserPagesAsync(Guid userId);
}
