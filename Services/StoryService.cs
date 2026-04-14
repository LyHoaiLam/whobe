using whobe.DTOs.Story;
using whobe.Interfaces;
using whobe.Models;

namespace whobe.Services;
//đăng story, xem, react, group theo user cho newsfeed
public class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepo;
    private readonly IFileStorageService _storage;

    public StoryService(IStoryRepository storyRepo, IFileStorageService storage) {
        _storyRepo = storyRepo;
        _storage = storage;
    }

    public async Task<StoryResponseDto> CreateAsync(Guid userId, CreateStoryDto dto) {
        string? mediaUrl = null;
        if (dto.Media != null) {
            if (dto.MediaType == 1 && !_storage.IsValidVideo(dto.Media))
                throw new Exception("File video không hợp lệ.");
            if (dto.MediaType == 0 && !_storage.IsValidImage(dto.Media))
                throw new Exception("File ảnh không hợp lệ.");

            mediaUrl = await _storage.UploadAsync(dto.Media, "stories");
        }

        var story = new Story {
            UserId = userId,
            MediaUrl = mediaUrl,
            MediaType = (byte)dto.MediaType,
            TextContent = dto.TextContent,
            TextStyle = dto.TextStyle,
            PrivacyLevel = dto.PrivacyLevel,
        };

        await _storyRepo.AddAsync(story);
        await _storyRepo.SaveChangesAsync();

        return MapToDto(story, 0, false);
    }

    public async Task<IEnumerable<StoryGroupDto>> GetFriendsStoriesAsync(Guid userId) {
        var stories = await _storyRepo.GetFriendsStoriesAsync(userId);

        return stories.GroupBy(s => s.UserId).Select(g => {
                var user = g.First().User;
                return new StoryGroupDto {
                    UserId = g.Key,
                    UserName = user?.FullName ?? "",
                    UserAvatar = user?.AvatarUrl,
                    HasUnviewed = g.Any(s => !s.Views.Any(v => v.ViewerId == userId)),
                    Stories = g.Select(s =>
                        MapToDto(s, s.Views.Count, s.Views.Any(v => v.ViewerId == userId))
                    ).ToList(),
                };
            });
    }

    public async Task<IEnumerable<StoryResponseDto>> GetUserStoriesAsync(Guid profileUserId, Guid currentUserId) {
        var stories = await _storyRepo.GetActiveStoriesOfUserAsync(profileUserId);
        return stories.Select(s =>
            MapToDto(s, s.Views.Count, s.Views.Any(v => v.ViewerId == currentUserId)));
    }

    public async Task ViewAsync(Guid storyId, Guid viewerId) {
        var already = await _storyRepo.HasViewedAsync(storyId, viewerId);
        if (already) return;

        var story = await _storyRepo.GetByIdAsync(storyId)
            ?? throw new Exception("Story không tồn tại.");

        if (story.ExpiresAt <= DateTime.UtcNow)
            throw new Exception("Story đã hết hạn.");

        story.Views.Add(new StoryView { StoryId = storyId, ViewerId = viewerId });
        await _storyRepo.SaveChangesAsync();
    }

    public async Task ReactAsync(Guid storyId, Guid viewerId, byte reactionType) {
        var story = await _storyRepo.GetByIdAsync(storyId)
            ?? throw new Exception("Story không tồn tại.");

        var view = story.Views.FirstOrDefault(v => v.ViewerId == viewerId);
        if (view == null) {
            story.Views.Add(new StoryView {
                StoryId = storyId,
                ViewerId = viewerId,
                ReactionType = reactionType,
            });
        } else {
            view.ReactionType = reactionType;
        }
        await _storyRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid storyId, Guid userId) {
        var story = await _storyRepo.GetByIdAsync(storyId)
            ?? throw new Exception("Story không tồn tại.");

        if (story.UserId != userId)
            throw new Exception("Bạn không có quyền xóa story này.");

        await _storyRepo.DeleteAsync(story);
        await _storyRepo.SaveChangesAsync();

        if (story.MediaUrl != null)
            await _storage.DeleteAsync(story.MediaUrl);
    }

    public async Task<IEnumerable<StoryViewerDto>> GetViewersAsync(Guid storyId, Guid ownerId) {
        var story = await _storyRepo.GetByIdAsync(storyId)
            ?? throw new Exception("Story không tồn tại.");

        if (story.UserId != ownerId)
            throw new Exception("Bạn không có quyền xem danh sách người xem.");

        return story.Views.Select(v => new StoryViewerDto {
            ViewerId = v.ViewerId,
            ViewerName = v.Viewer?.FullName ?? "",
            ViewerAvatar = v.Viewer?.AvatarUrl,
            ReactionType = v.ReactionType,
            ViewedAt = v.ViewedAt,
        });
    }

    private static StoryResponseDto MapToDto(Story s, int viewCount, bool isViewed) => new() {
        StoryId = s.StoryId,
        UserId = s.UserId,
        UserName = s.User?.FullName ?? "",
        UserAvatar = s.User?.AvatarUrl,
        MediaUrl = s.MediaUrl,
        MediaType = s.MediaType,
        TextContent = s.TextContent,
        ViewCount = viewCount,
        IsViewed = isViewed,
        ExpiresAt = s.ExpiresAt,
        CreatedAt = s.CreatedAt,
    };
}
