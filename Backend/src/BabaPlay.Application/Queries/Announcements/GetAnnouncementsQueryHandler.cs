using System.Collections.Generic;
using System.Linq;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Announcements;

public sealed class GetAnnouncementsQueryHandler
    : IQueryHandler<GetAnnouncementsQuery, Result<IReadOnlyList<AnnouncementResponse>>>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public GetAnnouncementsQueryHandler(
        IAnnouncementRepository announcementRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _announcementRepository = announcementRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<AnnouncementResponse>>> HandleAsync(
        GetAnnouncementsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;

        var announcements = await _announcementRepository.GetByTenantAsync(
            tenantId,
            query.OnlyPublished,
            cancellationToken);

        var now = DateTime.UtcNow;
        var newThreshold = now.AddHours(-24);

        var responses = new List<AnnouncementResponse>(announcements.Count);

        foreach (var a in announcements)
        {
            var isRead = false;
            if (!string.IsNullOrWhiteSpace(query.RequestingUserId))
            {
                isRead = await _announcementRepository.HasUserReadAsync(
                    a.Id,
                    query.RequestingUserId,
                    cancellationToken);
            }

            // Resolve author display name via user repository
            var author = await _userRepository.FindByIdAsync(a.AuthorId.ToString(), cancellationToken);
            var authorName = author?.Email ?? "Diretoria";

            var isNew = a.PublishedAtUtc.HasValue && a.PublishedAtUtc.Value >= newThreshold;

            responses.Add(new AnnouncementResponse(
                a.Id,
                a.TenantId,
                a.AuthorId,
                authorName,
                a.Title,
                a.Content,
                a.IsPublished,
                a.PublishedAtUtc,
                a.ExpiresAtUtc,
                a.Tags,
                isRead,
                isNew,
                a.CreatedAt));
        }

        return Result<IReadOnlyList<AnnouncementResponse>>.Ok(responses);
    }
}
