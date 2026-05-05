using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class UserDeviceTokenTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveToken()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var token = UserDeviceToken.Create(tenantId, userId, "device-123", "fcm-token", "android");

        token.TenantId.Should().Be(tenantId);
        token.UserId.Should().Be(userId);
        token.DeviceId.Should().Be("device-123");
        token.Token.Should().Be("fcm-token");
        token.Platform.Should().Be("android");
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ShouldThrowValidationException()
    {
        var act = () => UserDeviceToken.Create(Guid.Empty, Guid.NewGuid(), "device-123", "fcm-token", "android");

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void RotateToken_ValidToken_ShouldUpdateValueAndUpdatedAt()
    {
        var token = UserDeviceToken.Create(Guid.NewGuid(), Guid.NewGuid(), "device-123", "old-token", "android");

        token.RotateToken("new-token");

        token.Token.Should().Be("new-token");
        token.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var token = UserDeviceToken.Create(Guid.NewGuid(), Guid.NewGuid(), "device-123", "fcm-token", "android");
        token.Deactivate();

        var act = () => token.Deactivate();

        act.Should().NotThrow();
        token.IsActive.Should().BeFalse();
    }
}
