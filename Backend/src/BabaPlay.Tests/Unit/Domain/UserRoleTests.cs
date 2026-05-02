using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class UserRoleTests
{
    [Fact]
    public void Create_ValidData_ReturnsAssignment()
    {
        var roleId = Guid.NewGuid();

        var assignment = UserRole.Create("user-123", roleId);

        assignment.UserId.Should().Be("user-123");
        assignment.RoleId.Should().Be(roleId);
        assignment.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WhitespaceUserId_ThrowsValidationException()
    {
        var act = () => UserRole.Create("  ", Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyRoleId_ThrowsValidationException()
    {
        var act = () => UserRole.Create("user-123", Guid.Empty);

        act.Should().Throw<ValidationException>();
    }
}
