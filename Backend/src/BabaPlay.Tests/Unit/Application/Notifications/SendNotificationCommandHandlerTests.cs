using System;
using System.Collections.Generic;
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

public class SendNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepository = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly SendNotificationCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    public SendNotificationCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);
        _handler = new SendNotificationCommandHandler(
            _notificationRepository.Object,
            _userTenantRepository.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WhenTitleMissing_ShouldReturnInvalidTitle()
    {
        var command = new SendNotificationCommand(string.Empty, "Message");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TITLE");
    }

    [Fact]
    public async Task Handle_WhenTenantMissing_ShouldReturnInvalidTenant()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(Guid.Empty);

        var command = new SendNotificationCommand("Title", "Message");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TENANT");
    }

    [Fact]
    public async Task Handle_WhenBroadcast_ShouldCreateNotificationForEachMember()
    {
        _userTenantRepository
            .Setup(x => x.GetMemberUserIdsAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { UserA, UserB });

        var command = new SendNotificationCommand("Aviso", "Mensagem", NotificationType.System);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CreatedCount.Should().Be(2);
        _notificationRepository.Verify(
            x => x.AddRangeAsync(
                It.Is<IReadOnlyList<Notification>>(list => list.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTargetUsersProvided_ShouldFilterToTenantMembers()
    {
        var outsider = Guid.NewGuid();

        _userTenantRepository
            .Setup(x => x.GetMemberUserIdsAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { UserA, UserB });

        var command = new SendNotificationCommand(
            "Aviso",
            "Mensagem",
            NotificationType.System,
            [UserA, outsider]);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CreatedCount.Should().Be(1);
        _notificationRepository.Verify(
            x => x.AddRangeAsync(
                It.Is<IReadOnlyList<Notification>>(list =>
                    list.Count == 1 && list[0].UserId == UserA),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoValidTargets_ShouldReturnNoRecipients()
    {
        _userTenantRepository
            .Setup(x => x.GetMemberUserIdsAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { UserA });

        var command = new SendNotificationCommand(
            "Aviso",
            "Mensagem",
            NotificationType.System,
            [Guid.NewGuid()]);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NO_RECIPIENTS");
    }
}
