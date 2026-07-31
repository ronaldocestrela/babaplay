using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Repository contract for the ChatMessage entity (F27 — Real-Time Team Chat).
/// </summary>
public interface IChatMessageRepository
{
    /// <summary>Returns recent chat messages for a tenant in chronological order (oldest first).</summary>
    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(
        Guid tenantId,
        int limit = 50,
        CancellationToken ct = default);

    /// <summary>Tracks a new chat message for persistence.</summary>
    Task AddAsync(ChatMessage message, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
