using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetPlayerStatementQueryHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _repo = new();
    private readonly GetPlayerStatementQueryHandler _handler;

    public GetPlayerStatementQueryHandlerTests()
    {
        _handler = new GetPlayerStatementQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_EmptyPlayerId_ShouldReturnValidationError()
    {
        var fromUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc);

        var result = await _handler.HandleAsync(new GetPlayerStatementQuery(Guid.Empty, fromUtc, toUtc));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FINANCIAL_INVALID_PLAYER_ID");
    }

    [Fact]
    public async Task Handle_InvalidPeriod_ShouldReturnValidationError()
    {
        var fromUtc = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await _handler.HandleAsync(new GetPlayerStatementQuery(Guid.NewGuid(), fromUtc, toUtc));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnStatementTotals()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var fromUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 5, 31, 23, 59, 59, DateTimeKind.Utc);

        var fee1 = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), null);
        fee1.ApplyPayment(30m, new DateTime(2026, 5, 9, 0, 0, 0, DateTimeKind.Utc));

        var fee2 = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 5, 80m, new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), null);

        _repo.Setup(x => x.GetByPlayerAndPeriodAsync(playerId, fromUtc, toUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync([fee1, fee2]);

        var result = await _handler.HandleAsync(new GetPlayerStatementQuery(playerId, fromUtc, toUtc));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalAmount.Should().Be(180m);
        result.Value.TotalPaid.Should().Be(30m);
        result.Value.TotalOpen.Should().Be(150m);
        result.Value.Items.Should().HaveCount(2);
    }
}
