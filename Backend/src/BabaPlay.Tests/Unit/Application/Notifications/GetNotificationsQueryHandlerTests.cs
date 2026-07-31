using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Notifications;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Notifications;

public class GetNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GetNotificationsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetNotificationsQueryHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new GetNotificationsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnInvalidUserError()
    {
        var query = new GetNotificationsQuery(Guid.Empty);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_USER");
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnNotificationsAndUnreadCount()
    {
        var notification = Notification.Create(
            TenantId,
            UserId,
            NotificationType.Checkin,
            "Check-in Confirmado",
            "Sua presença no jogo de sábado foi confirmada.",
            null);

        _repo
            .Setup(x => x.GetByUserAsync(TenantId, UserId, false, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([notification]);

        _repo
            .Setup(x => x.GetUnreadCountAsync(TenantId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetNotificationsQuery(UserId);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UnreadCount.Should().Be(1);
        result.Value.Notifications.Should().HaveCount(1);
        result.Value.Notifications[0].Title.Should().Be("Check-in Confirmado");
    }
}
