using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PermissionTests
{
    [Fact]
    public void Create_ValidData_ReturnsPermission()
    {
        var permission = Permission.Create("player.create", "Can create players");

        permission.Id.Should().NotBeEmpty();
        permission.Code.Should().Be("player.create");
        permission.NormalizedCode.Should().Be("PLAYER.CREATE");
        permission.Description.Should().Be("Can create players");
        permission.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Create_WhitespaceCode_ThrowsValidationException()
    {
        var act = () => Permission.Create("  ", null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TrimsCodeAndDescription()
    {
        var permission = Permission.Create("  match.update  ", "  Update matches  ");

        permission.Code.Should().Be("match.update");
        permission.NormalizedCode.Should().Be("MATCH.UPDATE");
        permission.Description.Should().Be("Update matches");
    }
}
