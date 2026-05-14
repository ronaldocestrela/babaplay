using BabaPlay.Application.Commands.Notifications;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Notifications;

public class UpdateNotificationPreferencesCommandHandlerTests
{
    private readonly Mock<IUserNotificationPreferencesRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly UpdateNotificationPreferencesCommandHandler _handler;

    public UpdateNotificationPreferencesCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new UpdateNotificationPreferencesCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyUserId_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new UpdateNotificationPreferencesCommand(
            Guid.Empty,
            true,
            true,
            true,
            true,
            true));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOTIFICATION_INVALID_USER_ID");
    }

    [Fact]
    public async Task Handle_NotExistingPreferences_ShouldCreateAndUpdate()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var userId = Guid.NewGuid();

        _repo
            .Setup(x => x.GetByUserAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserNotificationPreferences?)null);

        var result = await _handler.HandleAsync(new UpdateNotificationPreferencesCommand(
            userId,
            false,
            true,
            false,
            true,
            false));

        result.IsSuccess.Should().BeTrue();
        result.Value!.PushEnabled.Should().BeFalse();
        result.Value.MatchEnabled.Should().BeFalse();
        _repo.Verify(x => x.AddAsync(It.IsAny<UserNotificationPreferences>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingPreferences_ShouldUpdate()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var userId = Guid.NewGuid();
        var existing = UserNotificationPreferences.CreateDefault(tenantId, userId);

        _repo
            .Setup(x => x.GetByUserAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.HandleAsync(new UpdateNotificationPreferencesCommand(
            userId,
            true,
            false,
            true,
            false,
            true));

        result.IsSuccess.Should().BeTrue();
        result.Value!.CheckinEnabled.Should().BeFalse();
        result.Value.MatchEventEnabled.Should().BeFalse();
        _repo.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
