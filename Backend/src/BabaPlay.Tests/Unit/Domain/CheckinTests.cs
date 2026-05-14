using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class CheckinTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveCheckin()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var gameDayId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var checkedInAt = scheduledAt.Date.AddHours(8);

        var checkin = Checkin.Create(
            tenantId,
            playerId,
            gameDayId,
            checkedInAt,
            -23.5505,
            -46.6333,
            25.8);

        checkin.TenantId.Should().Be(tenantId);
        checkin.PlayerId.Should().Be(playerId);
        checkin.GameDayId.Should().Be(gameDayId);
        checkin.CheckedInAtUtc.Should().Be(checkedInAt);
        checkin.Latitude.Should().Be(-23.5505);
        checkin.Longitude.Should().Be(-46.6333);
        checkin.DistanceFromAssociationMeters.Should().Be(25.8);
        checkin.IsActive.Should().BeTrue();
        checkin.CancelledAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_InvalidLatitude_ShouldThrowValidationException()
    {
        var act = () => Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1).AddHours(8),
            -100,
            -46.6333,
            10);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeDistance_ShouldThrowValidationException()
    {
        var act = () => Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1).AddHours(8),
            -23.5505,
            -46.6333,
            -0.1);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var checkin = Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1).AddHours(8),
            -23.5505,
            -46.6333,
            15);

        checkin.Deactivate(DateTime.UtcNow);
        var firstCancelledAt = checkin.CancelledAtUtc;

        checkin.Deactivate(DateTime.UtcNow.AddMinutes(1));

        checkin.IsActive.Should().BeFalse();
        checkin.CancelledAtUtc.Should().Be(firstCancelledAt);
    }
}
