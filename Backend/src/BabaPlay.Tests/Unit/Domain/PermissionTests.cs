using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PermissionTests
{
    [Fact]
    public void Create_ValidData_ReturnsPermission()
    {
        var tenantId = Guid.NewGuid();
        var permission = Permission.Create(tenantId, "player.create", "Can create players");

        permission.Id.Should().NotBeEmpty();
        permission.TenantId.Should().Be(tenantId);
        permission.Code.Should().Be("player.create");
        permission.NormalizedCode.Should().Be("PLAYER.CREATE");
        permission.Description.Should().Be("Can create players");
        permission.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Create_WhitespaceCode_ThrowsValidationException()
    {
        var act = () => Permission.Create(Guid.NewGuid(), "  ", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsValidationException()
    {
        var act = () => Permission.Create(Guid.Empty, "player.create", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TrimsCodeAndDescription()
    {
        var permission = Permission.Create(Guid.NewGuid(), "  match.update  ", "  Update matches  ");

        permission.Code.Should().Be("match.update");
        permission.NormalizedCode.Should().Be("MATCH.UPDATE");
        permission.Description.Should().Be("Update matches");
    }
}
