using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Announcements;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Announcements;

public class GetAnnouncementsQueryHandlerTests
{
    private readonly Mock<IAnnouncementRepository> _announcementRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GetAnnouncementsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private const string UserId = "user-abc";

    public GetAnnouncementsQueryHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _userRepo
            .Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "autor@babaplay.com", true));

        _handler = new GetAnnouncementsQueryHandler(
            _announcementRepo.Object,
            _userRepo.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithPublishedAnnouncements_ShouldReturnMappedDtos()
    {
        var announcement = Announcement.Create(TenantId, AuthorId, "Aviso Importante", "Conteúdo do aviso");
        announcement.Publish();

        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement> { announcement });

        _announcementRepo
            .Setup(x => x.HasUserReadAsync(announcement.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var query = new GetAnnouncementsQuery(OnlyPublished: true, RequestingUserId: UserId);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].Title.Should().Be("Aviso Importante");
        result.Value[0].IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserHasRead_ShouldReturnIsReadTrue()
    {
        var announcement = Announcement.Create(TenantId, AuthorId, "Aviso Lido", "Conteúdo");
        announcement.Publish();

        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement> { announcement });

        _announcementRepo
            .Setup(x => x.HasUserReadAsync(announcement.Id, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // already read

        var query = new GetAnnouncementsQuery(RequestingUserId: UserId);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value![0].IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenPublishedWithinLast24Hours_ShouldReturnIsNewTrue()
    {
        var announcement = Announcement.Create(TenantId, AuthorId, "Novo Aviso", "Conteúdo recente");
        announcement.Publish(); // PublishedAtUtc = UtcNow

        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement> { announcement });

        _announcementRepo
            .Setup(x => x.HasUserReadAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var query = new GetAnnouncementsQuery();

        var result = await _handler.HandleAsync(query);

        result.Value![0].IsNew.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoAnnouncements_ShouldReturnEmptyList()
    {
        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement>());

        var query = new GetAnnouncementsQuery();

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
