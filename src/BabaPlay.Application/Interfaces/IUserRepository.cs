using BabaPlay.Application.DTOs;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Read-only abstraction over the identity user store for authentication use cases.
/// Implementations live in Infrastructure; Application depends only on this interface.
/// </summary>
public interface IUserRepository
{
    Task<UserAuthDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAuthDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);
}
