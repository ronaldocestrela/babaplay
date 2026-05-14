using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class TenantGameDayOptionTests
{
    [Fact]
    public void Create_ValidData_ShouldReturnActiveOption()
    {
        var tenantId = Guid.NewGuid();

        var option = TenantGameDayOption.Create(tenantId, DayOfWeek.Tuesday, new TimeOnly(20, 0));

        option.Id.Should().NotBeEmpty();
        option.TenantId.Should().Be(tenantId);
        option.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        option.LocalStartTime.Should().Be(new TimeOnly(20, 0));
        option.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ShouldThrowValidationException()
    {
        var act = () => TenantGameDayOption.Create(Guid.Empty, DayOfWeek.Saturday, new TimeOnly(9, 30));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var option = TenantGameDayOption.Create(Guid.NewGuid(), DayOfWeek.Saturday, new TimeOnly(9, 0));

        option.Deactivate();
        var act = () => option.Deactivate();

        act.Should().NotThrow();
        option.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveTrue()
    {
        var option = TenantGameDayOption.Create(Guid.NewGuid(), DayOfWeek.Sunday, new TimeOnly(10, 0));
        option.Deactivate();

        option.Activate();

        option.IsActive.Should().BeTrue();
        option.UpdatedAt.Should().NotBeNull();
    }
}
