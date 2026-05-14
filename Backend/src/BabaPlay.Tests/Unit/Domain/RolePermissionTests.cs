using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class RolePermissionTests
{
    [Fact]
    public void Create_ValidIds_ReturnsLink()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var link = RolePermission.Create(roleId, permissionId);

        link.RoleId.Should().Be(roleId);
        link.PermissionId.Should().Be(permissionId);
        link.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyRoleId_ThrowsValidationException()
    {
        var act = () => RolePermission.Create(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyPermissionId_ThrowsValidationException()
    {
        var act = () => RolePermission.Create(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ValidationException>();
    }
}
