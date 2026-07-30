using BabaPlay.Application.Queries.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetDefaultersListQueryHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly GetDefaultersListQueryHandler _handler;

    public GetDefaultersListQueryHandlerTests()
    {
        _handler = new GetDefaultersListQueryHandler(_monthlyFeeRepo.Object, _playerRepo.Object);
    }

    [Fact]
    public async Task Handle_NoOverdueFees_ShouldReturnEmptyList()
    {
        _monthlyFeeRepo.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMonthlyFee>());

        var result = await _handler.HandleAsync(new GetDefaultersListQuery(DateTime.UtcNow));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalDefaultersCount.Should().Be(0);
        result.Value.TotalOverdueAmount.Should().Be(0m);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithOverdueFees_ShouldGroupAndOrderCorrectly()
    {
        var tenantId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var player1 = Player.Create(tenantId, Guid.NewGuid(), "Carlos Silva", "carlos@test.com", "71999998888", null);
        typeof(Player).GetProperty(nameof(Player.Id))!.SetValue(player1, player1Id);

        var player2 = Player.Create(tenantId, Guid.NewGuid(), "Bruno Santos", "bruno@test.com", "71988887777", null);
        typeof(Player).GetProperty(nameof(Player.Id))!.SetValue(player2, player2Id);

        var refDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var fee1 = PlayerMonthlyFee.Create(tenantId, player1Id, 2026, 4, 100m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), "Abril");
        var fee2 = PlayerMonthlyFee.Create(tenantId, player1Id, 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Maio");
        var fee3 = PlayerMonthlyFee.Create(tenantId, player2Id, 2026, 5, 150m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Maio");

        _monthlyFeeRepo.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMonthlyFee> { fee1, fee2, fee3 });

        _playerRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player1, player2 });

        var result = await _handler.HandleAsync(new GetDefaultersListQuery(refDate));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalDefaultersCount.Should().Be(2);
        result.Value.TotalOverdueAmount.Should().Be(350m);
        result.Value.Items.Should().HaveCount(2);

        // First item should be player 1 (Carlos, 52 days overdue)
        result.Value.Items[0].PlayerId.Should().Be(player1Id);
        result.Value.Items[0].PlayerName.Should().Be("Carlos Silva");
        result.Value.Items[0].OverdueInvoicesCount.Should().Be(2);
        result.Value.Items[0].TotalOverdueAmount.Should().Be(200m);
        result.Value.Items[0].MaxDaysOverdue.Should().Be(52);
    }
}
