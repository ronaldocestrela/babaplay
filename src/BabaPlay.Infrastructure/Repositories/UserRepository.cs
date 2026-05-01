using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<UserAuthDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserAuthDto(user.Id, user.Email!, user.IsActive);
    }

    public async Task<UserAuthDto?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user is null ? null : new UserAuthDto(user.Id, user.Email!, user.IsActive);
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Array.Empty<string>();
        return (await _userManager.GetRolesAsync(user)).ToArray();
    }
}
