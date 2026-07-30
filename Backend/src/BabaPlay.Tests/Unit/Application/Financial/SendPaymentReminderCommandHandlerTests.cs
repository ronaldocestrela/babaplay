using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class SendPaymentReminderCommandHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly SendPaymentReminderCommandHandler _handler;

    public SendPaymentReminderCommandHandlerTests()
    {
        _handler = new SendPaymentReminderCommandHandler(_playerRepo.Object, _monthlyFeeRepo.Object);
    }

    [Fact]
    public async Task Handle_PlayerNotFound_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new SendPaymentReminderCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_PlayerNoOverdueFees_ShouldReturnError()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = Player.Create(tenantId, Guid.NewGuid(), "Carlos Silva", "carlos@test.com", "71999998888", null);

        _playerRepo.Setup(x => x.GetByIdAsync(playerId, It.IsAny<CancellationToken>())).ReturnsAsync(player);
        _monthlyFeeRepo.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlayerMonthlyFee>());

        var result = await _handler.HandleAsync(new SendPaymentReminderCommand(playerId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NO_OVERDUE_FEES");
    }

    [Fact]
    public async Task Handle_ValidPlayerWithOverdueFees_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = Player.Create(tenantId, Guid.NewGuid(), "Carlos Silva", "carlos@test.com", "71999998888", null);
        var fee = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 4, 100m, DateTime.UtcNow.AddDays(-10), "Maio");

        _playerRepo.Setup(x => x.GetByIdAsync(playerId, It.IsAny<CancellationToken>())).ReturnsAsync(player);
        _monthlyFeeRepo.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PlayerMonthlyFee> { fee });

        var result = await _handler.HandleAsync(new SendPaymentReminderCommand(playerId));

        result.IsSuccess.Should().BeTrue();
    }
}
