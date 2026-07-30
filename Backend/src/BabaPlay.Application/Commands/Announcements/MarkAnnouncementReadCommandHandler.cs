using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Announcements;

public sealed class MarkAnnouncementReadCommandHandler
    : ICommandHandler<MarkAnnouncementReadCommand, Result>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ITenantContext _tenantContext;

    public MarkAnnouncementReadCommandHandler(
        IAnnouncementRepository announcementRepository,
        ITenantContext tenantContext)
    {
        _announcementRepository = announcementRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(
        MarkAnnouncementReadCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.AnnouncementId == Guid.Empty)
            return Result.Fail("INVALID_ANNOUNCEMENT", "AnnouncementId is required.");

        if (string.IsNullOrWhiteSpace(command.UserId))
            return Result.Fail("INVALID_USER", "UserId is required.");

        var tenantId = _tenantContext.TenantId;

        // Validate the announcement belongs to the current tenant
        var announcement = await _announcementRepository.GetByIdAsync(
            command.AnnouncementId,
            tenantId,
            cancellationToken);

        if (announcement is null)
            return Result.Fail("NOT_FOUND", "Announcement not found or does not belong to the current tenant.");

        // Idempotency check — do not insert duplicates
        var alreadyRead = await _announcementRepository.HasUserReadAsync(
            command.AnnouncementId,
            command.UserId,
            cancellationToken);

        if (alreadyRead)
            return Result.Ok();

        var readRecord = AnnouncementRead.Create(command.AnnouncementId, command.UserId);
        await _announcementRepository.AddReadAsync(readRecord, cancellationToken);

        return Result.Ok();
    }
}
