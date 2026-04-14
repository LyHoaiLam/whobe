using whobe.DTOs.Story;

namespace whobe.Interfaces;

public interface IStoryService
{
    Task<StoryResponseDto> CreateAsync(Guid userId, CreateStoryDto dto);
    Task<IEnumerable<StoryGroupDto>> GetFriendsStoriesAsync(Guid userId);
    Task<IEnumerable<StoryResponseDto>> GetUserStoriesAsync(Guid profileUserId, Guid currentUserId);
    Task ViewAsync(Guid storyId, Guid viewerId);
    Task ReactAsync(Guid storyId, Guid viewerId, byte reactionType);
    Task DeleteAsync(Guid storyId, Guid userId);
    Task<IEnumerable<StoryViewerDto>> GetViewersAsync(Guid storyId, Guid ownerId);
}
