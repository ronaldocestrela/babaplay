using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class UserNotificationPreferencesTests
{
    [Fact]
    public void CreateDefault_ValidData_ShouldCreateEnabledPreferences()
    {
        var prefs = UserNotificationPreferences.CreateDefault(Guid.NewGuid(), Guid.NewGuid());

        prefs.PushEnabled.Should().BeTrue();
        prefs.CheckinEnabled.Should().BeTrue();
        prefs.MatchEnabled.Should().BeTrue();
        prefs.MatchEventEnabled.Should().BeTrue();
        prefs.GameDayEnabled.Should().BeTrue();
        prefs.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateDefault_EmptyUserId_ShouldThrowValidationException()
    {
        var act = () => UserNotificationPreferences.CreateDefault(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_WhenValuesChange_ShouldPersistFlagsAndUpdatedAt()
    {
        var prefs = UserNotificationPreferences.CreateDefault(Guid.NewGuid(), Guid.NewGuid());

        prefs.Update(pushEnabled: false, checkinEnabled: true, matchEnabled: false, matchEventEnabled: true, gameDayEnabled: false);

        prefs.PushEnabled.Should().BeFalse();
        prefs.CheckinEnabled.Should().BeTrue();
        prefs.MatchEnabled.Should().BeFalse();
        prefs.MatchEventEnabled.Should().BeTrue();
        prefs.GameDayEnabled.Should().BeFalse();
        prefs.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var prefs = UserNotificationPreferences.CreateDefault(Guid.NewGuid(), Guid.NewGuid());
        prefs.Deactivate();

        var act = () => prefs.Deactivate();

        act.Should().NotThrow();
        prefs.IsActive.Should().BeFalse();
    }
}
