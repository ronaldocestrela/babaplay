using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Announcements;

public sealed class CreateAnnouncementCommandHandler
    : ICommandHandler<CreateAnnouncementCommand, Result<AnnouncementResponse>>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public CreateAnnouncementCommandHandler(
        IAnnouncementRepository announcementRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _announcementRepository = announcementRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<AnnouncementResponse>> HandleAsync(
        CreateAnnouncementCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<AnnouncementResponse>.Fail("INVALID_TITLE", "Title is required.");

        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<AnnouncementResponse>.Fail("INVALID_CONTENT", "Content is required.");

        if (command.AuthorId == Guid.Empty)
            return Result<AnnouncementResponse>.Fail("INVALID_AUTHOR", "AuthorId is required.");

        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
            return Result<AnnouncementResponse>.Fail("INVALID_TENANT", "Tenant context is not resolved.");

        var announcement = Announcement.Create(
            tenantId,
            command.AuthorId,
            command.Title,
            command.Content,
            command.ExpiresAtUtc,
            command.Tags);

        // Publish immediately upon creation
        announcement.Publish();

        await _announcementRepository.AddAsync(announcement, cancellationToken);

        var author = await _userRepository.FindByIdAsync(command.AuthorId.ToString(), cancellationToken);
        var authorName = author?.Email ?? "Diretoria";

        var response = new AnnouncementResponse(
            announcement.Id,
            announcement.TenantId,
            announcement.AuthorId,
            authorName,
            announcement.Title,
            announcement.Content,
            announcement.IsPublished,
            announcement.PublishedAtUtc,
            announcement.ExpiresAtUtc,
            announcement.Tags,
            IsRead: false,
            IsNew: true,
            announcement.CreatedAt);

        return Result<AnnouncementResponse>.Ok(response);
    }
}
