using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Notifications;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Notifications;

public class MarkNotificationReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly MarkNotificationReadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid NotificationId = Guid.NewGuid();

    public MarkNotificationReadCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new MarkNotificationReadCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldReturnNotFoundError()
    {
        _repo
            .Setup(x => x.GetByIdAsync(NotificationId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var command = new MarkNotificationReadCommand(NotificationId, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenValidAndUnread_ShouldMarkAsReadAndUpdate()
    {
        var notification = Notification.Create(
            TenantId,
            UserId,
            NotificationType.Match,
            "Nova Convocação",
            "Você foi escalado para a partida de sábado.",
            null);

        _repo
            .Setup(x => x.GetByIdAsync(notification.Id, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var command = new MarkNotificationReadCommand(notification.Id, UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _repo.Verify(x => x.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }
}
