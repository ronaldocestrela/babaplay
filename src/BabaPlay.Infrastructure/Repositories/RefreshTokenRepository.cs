using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MasterDbContext _context;

    public RefreshTokenRepository(MasterDbContext context) => _context = context;

    public async Task AddAsync(string token, string userId, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<StoredRefreshTokenDto?> FindAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

        return entity is null
            ? null
            : new StoredRefreshTokenDto(entity.Token, entity.UserId, entity.ExpiresAt, entity.RevokedAt.HasValue);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

        if (entity is not null)
        {
            entity.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
