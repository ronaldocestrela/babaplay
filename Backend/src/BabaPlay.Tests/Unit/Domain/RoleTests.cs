using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class RoleTests
{
    [Fact]
    public void Create_ValidData_ReturnsActiveRole()
    {
        var tenantId = Guid.NewGuid();

        var role = Role.Create(tenantId, "Manager", "Can manage team data");

        role.Id.Should().NotBeEmpty();
        role.TenantId.Should().Be(tenantId);
        role.Name.Should().Be("Manager");
        role.NormalizedName.Should().Be("MANAGER");
        role.Description.Should().Be("Can manage team data");
        role.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsValidationException()
    {
        var act = () => Role.Create(Guid.Empty, "Manager", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WhitespaceName_ThrowsValidationException()
    {
        var act = () => Role.Create(Guid.NewGuid(), "  ", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TrimsAndNormalizesName()
    {
        var role = Role.Create(Guid.NewGuid(), "  team admin  ", null);

        role.Name.Should().Be("team admin");
        role.NormalizedName.Should().Be("TEAM ADMIN");
    }

    [Fact]
    public void AddPermission_NewPermission_AddsOnce()
    {
        var role = Role.Create(Guid.NewGuid(), "Manager", null);
        var permissionId = Guid.NewGuid();

        role.AddPermission(permissionId);

        role.Permissions.Should().ContainSingle(x => x.PermissionId == permissionId);
    }

    [Fact]
    public void AddPermission_DuplicatePermission_DoesNotDuplicate()
    {
        var role = Role.Create(Guid.NewGuid(), "Manager", null);
        var permissionId = Guid.NewGuid();
        role.AddPermission(permissionId);

        role.AddPermission(permissionId);

        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void RemovePermission_ExistingPermission_Removes()
    {
        var role = Role.Create(Guid.NewGuid(), "Manager", null);
        var permissionId = Guid.NewGuid();
        role.AddPermission(permissionId);

        role.RemovePermission(permissionId);

        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_IsIdempotent()
    {
        var role = Role.Create(Guid.NewGuid(), "Manager", null);
        role.Deactivate();

        var act = () => role.Deactivate();

        act.Should().NotThrow();
        role.IsActive.Should().BeFalse();
    }
}
