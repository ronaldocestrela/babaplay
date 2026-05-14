using BabaPlay.Application.Commands.Checkins;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Checkins;

public class CreateCheckinCommandHandlerTests
{
    private readonly Mock<ICheckinRepository> _checkinRepository = new();
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly Mock<IGameDayRepository> _gameDayRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly Mock<ITenantGeolocationSettingsRepository> _tenantGeolocationRepository = new();
    private readonly Mock<ICheckinRealtimeNotifier> _realtimeNotifier = new();
    private readonly CreateCheckinCommandHandler _handler;

    public CreateCheckinCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());

        _handler = new CreateCheckinCommandHandler(
            _checkinRepository.Object,
            _playerRepository.Object,
            _gameDayRepository.Object,
            _tenantContext.Object,
            _tenantGeolocationRepository.Object,
            _realtimeNotifier.Object);
    }

    [Fact]
    public async Task Handle_PlayerNotFound_ShouldReturnPlayerNotFound()
    {
        var command = BuildValidCommand();

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_PlayerInactive_ShouldReturnPlayerInactive()
    {
        var command = BuildValidCommand();
        var player = BuildOwnedPlayer(command);
        player.Deactivate();

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_INACTIVE");
    }

    [Fact]
    public async Task Handle_RequesterIsNotPlayerOwner_ShouldReturnForbidden()
    {
        var command = BuildValidCommand();

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Player.Create(Guid.NewGuid(), "Ronaldo", null, null, null));

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Handle_OutsideAllowedRadius_ShouldReturnOutsideRadius()
    {
        var command = BuildValidCommand() with { Latitude = -23.5610, Longitude = -46.7000 };
        var scheduledAt = command.CheckedInAtUtc.Date.AddHours(10);

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOwnedPlayer(command));

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(_tenantContext.Object.TenantId, "Rodada", scheduledAt, "Campo", null, 22));

        _tenantGeolocationRepository
            .Setup(x => x.GetSettingsAsync(_tenantContext.Object.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantGeolocationSettingsDto(-23.5505, -46.6333, 50));

        _checkinRepository
            .Setup(x => x.ExistsActiveByPlayerAndGameDayAsync(command.PlayerId, command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHECKIN_OUTSIDE_ALLOWED_RADIUS");
        _realtimeNotifier.Verify(x => x.NotifyCheckinDeniedAsync(
            command.GameDayId,
            command.PlayerId,
            "CHECKIN_OUTSIDE_ALLOWED_RADIUS",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Duplicate_ShouldReturnAlreadyExists()
    {
        var command = BuildValidCommand();
        var scheduledAt = command.CheckedInAtUtc.Date.AddHours(10);

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOwnedPlayer(command));

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(_tenantContext.Object.TenantId, "Rodada", scheduledAt, "Campo", null, 22));

        _tenantGeolocationRepository
            .Setup(x => x.GetSettingsAsync(_tenantContext.Object.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantGeolocationSettingsDto(command.Latitude, command.Longitude, 300));

        _checkinRepository
            .Setup(x => x.ExistsActiveByPlayerAndGameDayAsync(command.PlayerId, command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHECKIN_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCheckinAndNotify()
    {
        var command = BuildValidCommand();
        var scheduledAt = command.CheckedInAtUtc.Date.AddHours(10);

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOwnedPlayer(command));

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(_tenantContext.Object.TenantId, "Rodada", scheduledAt, "Campo", null, 22));

        _tenantGeolocationRepository
            .Setup(x => x.GetSettingsAsync(_tenantContext.Object.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantGeolocationSettingsDto(command.Latitude, command.Longitude, 300));

        _checkinRepository
            .Setup(x => x.ExistsActiveByPlayerAndGameDayAsync(command.PlayerId, command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PlayerId.Should().Be(command.PlayerId);

        _checkinRepository.Verify(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Once);
        _checkinRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyCheckinCreatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyCheckinCountUpdatedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CheckedInOnDifferentDay_ShouldReturnCheckinDayInvalid()
    {
        var command = BuildValidCommand() with { CheckedInAtUtc = DateTime.UtcNow.Date.AddDays(2).AddHours(8) };
        var scheduledAt = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

        _playerRepository
            .Setup(x => x.GetByIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOwnedPlayer(command));

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(command.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(_tenantContext.Object.TenantId, "Rodada", scheduledAt, "Campo", null, 22));

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHECKIN_DAY_INVALID");
    }

    private static CreateCheckinCommand BuildValidCommand()
    {
        var checkedInAt = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
        var requestedByUserId = Guid.NewGuid().ToString();

        return new CreateCheckinCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            checkedInAt,
            -23.5505,
            -46.6333,
            requestedByUserId);
    }

    private static Player BuildOwnedPlayer(CreateCheckinCommand command)
        => Player.Create(Guid.Parse(command.RequestedByUserId), "Ronaldo", null, null, null);
}
