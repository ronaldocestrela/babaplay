using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Announcements;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Announcements;

public class MarkAnnouncementReadCommandHandlerTests
{
    private readonly Mock<IAnnouncementRepository> _announcementRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly MarkAnnouncementReadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid AnnouncementId = Guid.NewGuid();
    private const string UserId = "user-xyz";

    public MarkAnnouncementReadCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new MarkAnnouncementReadCommandHandler(
            _announcementRepo.Object,
            _tenantContext.Object);
    }

    private Announcement BuildPublishedAnnouncement()
    {
        var a = Announcement.Create(TenantId, AuthorId, "Aviso", "Conteúdo");
        a.Publish();
        return a;
    }

    [Fact]
    public async Task Handle_WithEmptyAnnouncementId_ShouldReturnError()
    {
        var command = new MarkAnnouncementReadCommand(Guid.Empty, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ANNOUNCEMENT");
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnError()
    {
        var command = new MarkAnnouncementReadCommand(AnnouncementId, string.Empty);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_USER");
    }

    [Fact]
    public async Task Handle_WhenAnnouncementNotFound_ShouldReturnNotFoundError()
    {
        _announcementRepo
            .Setup(x => x.GetByIdAsync(AnnouncementId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Announcement?)null);

        var command = new MarkAnnouncementReadCommand(AnnouncementId, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenNotYetRead_ShouldInsertReadRecordAndReturnSuccess()
    {
        _announcementRepo
            .Setup(x => x.GetByIdAsync(AnnouncementId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPublishedAnnouncement());

        _announcementRepo
            .Setup(x => x.HasUserReadAsync(AnnouncementId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new MarkAnnouncementReadCommand(AnnouncementId, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _announcementRepo.Verify(
            x => x.AddReadAsync(It.IsAny<AnnouncementRead>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyRead_ShouldBeIdempotentAndNotInsertDuplicate()
    {
        _announcementRepo
            .Setup(x => x.GetByIdAsync(AnnouncementId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPublishedAnnouncement());

        _announcementRepo
            .Setup(x => x.HasUserReadAsync(AnnouncementId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // already read

        var command = new MarkAnnouncementReadCommand(AnnouncementId, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _announcementRepo.Verify(
            x => x.AddReadAsync(It.IsAny<AnnouncementRead>(), It.IsAny<CancellationToken>()),
            Times.Never); // idempotent — should NOT insert again
    }
}
