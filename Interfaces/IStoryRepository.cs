using whobe.Models;

namespace whobe.Interfaces;

public interface IStoryRepository : IGenericRepository<Story>
{
    Task<IEnumerable<Story>> GetActiveStoriesOfUserAsync(Guid userId);
    Task<IEnumerable<Story>> GetFriendsStoriesAsync(Guid userId);
    Task<bool> HasViewedAsync(Guid storyId, Guid viewerId);
    Task<int> CountViewsAsync(Guid storyId);
}
