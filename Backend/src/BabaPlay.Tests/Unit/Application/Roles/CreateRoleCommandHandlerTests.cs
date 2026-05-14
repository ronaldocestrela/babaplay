using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.IsResolved).Returns(true);
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateRoleCommandHandler(_roleRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WhitespaceName_ShouldReturnRoleNameRequired()
    {
        var result = await _handler.HandleAsync(new CreateRoleCommand("  ", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ROLE_NAME_REQUIRED");
    }

    [Fact]
    public async Task Handle_DuplicateRole_ShouldReturnRoleAlreadyExists()
    {
        _roleRepo.Setup(r => r.ExistsByNormalizedNameAsync("MANAGER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new CreateRoleCommand("Manager", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ROLE_ALREADY_EXISTS");
        _roleRepo.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateRole()
    {
        _roleRepo.Setup(r => r.ExistsByNormalizedNameAsync("MANAGER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateRoleCommand("Manager", "Desc"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Manager");
        _roleRepo.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _roleRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
