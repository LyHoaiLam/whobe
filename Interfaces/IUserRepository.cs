using whobe.Models;

namespace whobe.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
    Task<User?> GetProfileAsync(Guid userId);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<IEnumerable<User>> SearchUsersAsync(string keyword, int page, int pageSize);
    Task<IEnumerable<User>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<User>> GetFollowersAsync(Guid userId);
    Task<IEnumerable<User>> GetFollowingAsync(Guid userId);
}
