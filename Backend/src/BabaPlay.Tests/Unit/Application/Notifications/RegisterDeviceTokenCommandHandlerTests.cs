using BabaPlay.Application.Commands.Notifications;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Notifications;

public class RegisterDeviceTokenCommandHandlerTests
{
    private readonly Mock<IUserDeviceTokenRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly RegisterDeviceTokenCommandHandler _handler;

    public RegisterDeviceTokenCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new RegisterDeviceTokenCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyUserId_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new RegisterDeviceTokenCommand(
            Guid.Empty,
            "device-1",
            "token-1",
            "android"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOTIFICATION_INVALID_USER_ID");
    }

    [Fact]
    public async Task Handle_ExistingDevice_ShouldRotateTokenAndUpdate()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var userId = Guid.NewGuid();
        var existing = UserDeviceToken.Create(tenantId, userId, "device-1", "old-token", "android");

        _repo
            .Setup(x => x.GetByUserAndDeviceAsync(tenantId, userId, "device-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.HandleAsync(new RegisterDeviceTokenCommand(
            userId,
            "device-1",
            "new-token",
            "android"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.DeviceId.Should().Be("device-1");
        result.Value.Token.Should().Be("new-token");
        _repo.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.AddAsync(It.IsAny<UserDeviceToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewDevice_ShouldCreateToken()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var userId = Guid.NewGuid();

        _repo
            .Setup(x => x.GetByUserAndDeviceAsync(tenantId, userId, "device-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDeviceToken?)null);

        var result = await _handler.HandleAsync(new RegisterDeviceTokenCommand(
            userId,
            "device-2",
            "token-2",
            "ios"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Platform.Should().Be("ios");
        _repo.Verify(x => x.AddAsync(It.IsAny<UserDeviceToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
