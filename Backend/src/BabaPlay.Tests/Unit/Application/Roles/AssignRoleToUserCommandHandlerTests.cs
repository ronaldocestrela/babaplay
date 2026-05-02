using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Roles;

public class AssignRoleToUserCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _userRoleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();

    private readonly AssignRoleToUserCommandHandler _handler;

    public AssignRoleToUserCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.IsResolved).Returns(true);
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());

        _handler = new AssignRoleToUserCommandHandler(
            _roleRepo.Object,
            _userRoleRepo.Object,
            _userRepo.Object,
            _userTenantRepo.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnUserNotFound()
    {
        _userRepo.Setup(x => x.FindByIdAsync("u-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthDto?)null);

        var result = await _handler.HandleAsync(new AssignRoleToUserCommand("u-1", Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_RoleAlreadyAssigned_ShouldReturnRoleAlreadyAssigned()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin", null);
        var user = new UserAuthDto("u-1", "u1@test.com", true);

        _userRepo.Setup(x => x.FindByIdAsync("u-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userTenantRepo.Setup(x => x.IsMemberAsync("u-1", It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _roleRepo.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _userRoleRepo.Setup(x => x.ExistsAsync("u-1", role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.HandleAsync(new AssignRoleToUserCommand("u-1", role.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ROLE_ALREADY_ASSIGNED");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldAssignRole()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin", null);
        var user = new UserAuthDto("u-1", "u1@test.com", true);

        _userRepo.Setup(x => x.FindByIdAsync("u-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userTenantRepo.Setup(x => x.IsMemberAsync("u-1", It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _roleRepo.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _userRoleRepo.Setup(x => x.ExistsAsync("u-1", role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.HandleAsync(new AssignRoleToUserCommand("u-1", role.Id));

        result.IsSuccess.Should().BeTrue();
        _userRoleRepo.Verify(x => x.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRoleRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
