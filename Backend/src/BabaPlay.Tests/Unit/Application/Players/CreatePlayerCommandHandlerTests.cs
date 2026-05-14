using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class CreatePlayerCommandHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly CreatePlayerCommandHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidName = "João Silva";
    private static readonly UserAuthDto ValidUser = new(ValidUserId.ToString(), "test@babaplay.com", true);

    public CreatePlayerCommandHandlerTests()
    {
        _handler = new CreatePlayerCommandHandler(_playerRepo.Object, _userRepo.Object);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnInvalidName()
    {
        // Act
        var result = await _handler.HandleAsync(new CreatePlayerCommand(ValidUserId, "", null, null, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_NAME");
        _playerRepo.Verify(r => r.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnUserNotFound()
    {
        // Arrange
        _userRepo
            .Setup(r => r.FindByIdAsync(ValidUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthDto?)null);

        // Act
        var result = await _handler.HandleAsync(new CreatePlayerCommand(ValidUserId, ValidName, null, null, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_NOT_FOUND");
        _playerRepo.Verify(r => r.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateUserId_ShouldReturnPlayerAlreadyExists()
    {
        // Arrange
        _userRepo
            .Setup(r => r.FindByIdAsync(ValidUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidUser);
        _playerRepo
            .Setup(r => r.ExistsByUserIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(new CreatePlayerCommand(ValidUserId, ValidName, null, null, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_ALREADY_EXISTS");
        _playerRepo.Verify(r => r.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnPlayerResponse()
    {
        // Arrange
        _userRepo
            .Setup(r => r.FindByIdAsync(ValidUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidUser);
        _playerRepo
            .Setup(r => r.ExistsByUserIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dob = new DateOnly(1990, 5, 15);

        // Act
        var result = await _handler.HandleAsync(
            new CreatePlayerCommand(ValidUserId, ValidName, "Jão", "11999999999", dob));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be(ValidUserId);
        result.Value.Name.Should().Be(ValidName);
        result.Value.Nickname.Should().Be("Jão");
        result.Value.Phone.Should().Be("11999999999");
        result.Value.DateOfBirth.Should().Be(dob);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        _playerRepo.Verify(r => r.AddAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
        _playerRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldTrimName()
    {
        // Arrange
        _userRepo
            .Setup(r => r.FindByIdAsync(ValidUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidUser);
        _playerRepo
            .Setup(r => r.ExistsByUserIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(
            new CreatePlayerCommand(ValidUserId, "  João Silva  ", null, null, null));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("João Silva");
    }
}
