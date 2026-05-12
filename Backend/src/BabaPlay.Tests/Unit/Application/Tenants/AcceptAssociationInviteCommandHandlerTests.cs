using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Common;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Tenants;

public class AcceptAssociationInviteCommandHandlerTests
{
    private readonly Mock<IAssociationInviteRepository> _associationInviteRepository = new();
    private readonly Mock<ITenantRepository> _tenantRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserInvitationAccountService> _userInvitationAccountService = new();
    private readonly Mock<IUserTenantMembershipService> _userTenantMembershipService = new();

    private readonly AcceptAssociationInviteCommandHandler _handler;

    public AcceptAssociationInviteCommandHandlerTests()
    {
        _handler = new AcceptAssociationInviteCommandHandler(
            _associationInviteRepository.Object,
            _tenantRepository.Object,
            _userRepository.Object,
            _userInvitationAccountService.Object,
            _userTenantMembershipService.Object);
    }

    [Fact]
    public async Task Handle_Accept_WhenEmailMismatch_ShouldReturnConflictError()
    {
        var invite = CreateValidInvite();

        _associationInviteRepository
            .Setup(x => x.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userRepository
            .Setup(x => x.FindByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto("user-1", "other@club.com", true));

        var result = await _handler.HandleAsync(new AcceptAssociationInviteCommand("token", "user-1"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ASSOCIATION_INVITE_EMAIL_MISMATCH");
    }

    [Fact]
    public async Task Handle_RegisterAndAccept_WhenValid_ShouldCreateUserAndAccept()
    {
        var tenantId = Guid.NewGuid();
        var invite = CreateValidInvite(tenantId);

        _associationInviteRepository
            .Setup(x => x.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        _userRepository
            .Setup(x => x.FindByEmailAsync("invitee@club.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthDto?)null);

        _userInvitationAccountService
            .Setup(x => x.CreateUserAsync("invitee@club.com", "Password@123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok("new-user-id"));

        _tenantRepository
            .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantInfoDto(tenantId, "Club", "club", true, string.Empty, "Ready"));

        _userTenantMembershipService
            .Setup(x => x.EnsureMemberAsync("new-user-id", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new RegisterAndAcceptAssociationInviteCommand(
            "token",
            "invitee@club.com",
            "Password@123"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TenantId.Should().Be(tenantId);
        result.Value.UserId.Should().Be("new-user-id");

        _associationInviteRepository.Verify(x => x.MarkAcceptedAsync(invite.Id, "new-user-id", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AssociationInviteData CreateValidInvite(Guid? tenantId = null) => new(
        Guid.NewGuid(),
        tenantId ?? Guid.NewGuid(),
        "invitee@club.com",
        "INVITEE@CLUB.COM",
        "hash",
        DateTime.UtcNow.AddHours(23),
        DateTime.UtcNow,
        "owner-1",
        null,
        null,
        null);
}
