using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(
        Guid tenantId,
        int limit = 50,
        CancellationToken ct = default)
    {
        var rawMessages = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        // Reverse so that the returned list is in ascending chronological order (oldest to newest)
        rawMessages.Reverse();
        return rawMessages;
    }

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        await _context.ChatMessages.AddAsync(message, ct);
        await SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
