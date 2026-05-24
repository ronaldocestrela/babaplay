using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public sealed class RegisterManualPlayerCommandHandlerTests
{
    private readonly Mock<IUserInvitationAccountService> _userInvitationAccountService = new();
    private readonly Mock<IUserTenantMembershipService> _userTenantMembershipService = new();
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly RegisterManualPlayerCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public RegisterManualPlayerCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(TenantId);
        _handler = new RegisterManualPlayerCommandHandler(
            _userInvitationAccountService.Object,
            _userTenantMembershipService.Object,
            _playerRepository.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task HandleAsync_EmptyEmail_ShouldReturnEmailRequired()
    {
        var command = new RegisterManualPlayerCommand(
            "",
            "Temp1234",
            "Jogador",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MANUAL_PLAYER_EMAIL_REQUIRED");
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ShouldReturnEmailInvalid()
    {
        var command = new RegisterManualPlayerCommand(
            "invalid-email",
            "Temp1234",
            "Jogador",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MANUAL_PLAYER_EMAIL_INVALID");
    }

    [Fact]
    public async Task HandleAsync_EmptyPassword_ShouldReturnPasswordRequired()
    {
        var command = new RegisterManualPlayerCommand(
            "player@babaplay.com",
            "",
            "Jogador",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MANUAL_PLAYER_PASSWORD_REQUIRED");
    }

    [Fact]
    public async Task HandleAsync_EmptyName_ShouldReturnInvalidName()
    {
        var command = new RegisterManualPlayerCommand(
            "player@babaplay.com",
            "Temp1234",
            " ",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_NAME");
    }

    [Fact]
    public async Task HandleAsync_CreateUserFails_ShouldPropagateError()
    {
        _userInvitationAccountService
            .Setup(x => x.CreateUserAsync("player@babaplay.com", "Temp1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Fail("ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED", "already used"));

        var command = new RegisterManualPlayerCommand(
            "player@babaplay.com",
            "Temp1234",
            "Jogador",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED");
        _userTenantMembershipService.Verify(
            x => x.EnsureMemberAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PlayerAlreadyExists_ShouldReturnConflictWithoutSaving()
    {
        _userInvitationAccountService
            .Setup(x => x.CreateUserAsync("player@babaplay.com", "Temp1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(UserId.ToString()));
        _userTenantMembershipService
            .Setup(x => x.EnsureMemberAsync(UserId.ToString(), TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _playerRepository
            .Setup(x => x.ExistsByUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RegisterManualPlayerCommand(
            "player@babaplay.com",
            "Temp1234",
            "Jogador",
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_ALREADY_EXISTS");
        _playerRepository.Verify(x => x.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldCreatePlayerAndReturnResponse()
    {
        _userInvitationAccountService
            .Setup(x => x.CreateUserAsync("player@babaplay.com", "Temp1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(UserId.ToString()));
        _userTenantMembershipService
            .Setup(x => x.EnsureMemberAsync(UserId.ToString(), TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _playerRepository
            .Setup(x => x.ExistsByUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dateOfBirth = new DateOnly(1997, 8, 20);
        var command = new RegisterManualPlayerCommand(
            "player@babaplay.com",
            "Temp1234",
            "  Jogador Novo  ",
            " JN ",
            " 11999998888 ",
            dateOfBirth);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(UserId);
        result.Value.Name.Should().Be("Jogador Novo");
        result.Value.Nickname.Should().Be("JN");
        result.Value.Phone.Should().Be("11999998888");
        result.Value.DateOfBirth.Should().Be(dateOfBirth);

        _playerRepository.Verify(x => x.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
        _playerRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
