using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Roles;

public class AddPermissionToRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPermissionRepository> _permissionRepo = new();
    private readonly AddPermissionToRoleCommandHandler _handler;

    public AddPermissionToRoleCommandHandlerTests()
    {
        _handler = new AddPermissionToRoleCommandHandler(_roleRepo.Object, _permissionRepo.Object);
    }

    [Fact]
    public async Task Handle_RoleNotFound_ShouldReturnRoleNotFound()
    {
        _roleRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var result = await _handler.HandleAsync(new AddPermissionToRoleCommand(Guid.NewGuid(), "player.read", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ROLE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NewPermission_ShouldCreateAndAttach()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin", null);

        _roleRepo.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _permissionRepo.Setup(x => x.GetByNormalizedCodeAsync("PLAYER.READ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission?)null);

        var result = await _handler.HandleAsync(new AddPermissionToRoleCommand(role.Id, "player.read", "Read players"));

        result.IsSuccess.Should().BeTrue();
        _permissionRepo.Verify(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
        _roleRepo.Verify(x => x.UpdateAsync(role, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingPermission_ShouldNotCreateTwice()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin", null);
        var permission = Permission.Create("player.read", null);

        _roleRepo.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _permissionRepo.Setup(x => x.GetByNormalizedCodeAsync("PLAYER.READ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        var result = await _handler.HandleAsync(new AddPermissionToRoleCommand(role.Id, "player.read", null));

        result.IsSuccess.Should().BeTrue();
        _permissionRepo.Verify(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Never);
        result.Value!.PermissionIds.Should().Contain(permission.Id);
    }
}
