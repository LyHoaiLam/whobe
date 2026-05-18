using whobe.DTOs.Common;
using whobe.DTOs.Group;
using whobe.DTOs.User;
namespace whobe.Interfaces;

public interface IGroupService {
    Task<GroupResponseDto> CreateAsync(Guid userId, CreateGroupDto dto);
    Task<GroupResponseDto?> GetByIdAsync(Guid groupId, Guid currentUserId);
    Task<GroupResponseDto> UpdateAsync(Guid groupId, Guid userId, UpdateGroupDto dto);
    Task DeleteAsync(Guid groupId, Guid userId);
    Task JoinAsync(Guid groupId, Guid userId);
    Task LeaveAsync(Guid groupId, Guid userId);
    Task InviteAsync(Guid groupId, Guid inviterId, Guid inviteeId);
    Task KickMemberAsync(Guid groupId, Guid adminId, Guid memberId);
    Task PromoteToAdminAsync(Guid groupId, Guid ownerId, Guid memberId);
    Task<PagedResultDto<UserProfileDto>> GetMembersAsync(Guid groupId, int page, int pageSize);
    Task<PagedResultDto<GroupResponseDto>> SearchAsync(string keyword, int page, int pageSize);
}
