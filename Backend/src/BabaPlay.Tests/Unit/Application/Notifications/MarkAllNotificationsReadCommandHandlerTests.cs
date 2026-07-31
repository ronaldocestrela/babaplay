using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Commands.Notifications;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BabaPlay.Tests.Unit.Application.Notifications;

public class MarkAllNotificationsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly MarkAllNotificationsReadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public MarkAllNotificationsReadCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new MarkAllNotificationsReadCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnInvalidUserError()
    {
        var command = new MarkAllNotificationsReadCommand(Guid.Empty);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_USER");
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldCallRepositoryMarkAllAsRead()
    {
        var command = new MarkAllNotificationsReadCommand(UserId);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.MarkAllAsReadAsync(TenantId, UserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
