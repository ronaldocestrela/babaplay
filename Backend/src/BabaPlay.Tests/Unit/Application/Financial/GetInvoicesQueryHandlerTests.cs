using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetInvoicesQueryHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly GetInvoicesQueryHandler _handler;

    public GetInvoicesQueryHandlerTests()
    {
        _handler = new GetInvoicesQueryHandler(_monthlyFeeRepo.Object);
    }

    [Fact]
    public async Task Handle_InvalidYear_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetInvoicesQuery(0, 5, null, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_InvalidMonth_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetInvoicesQuery(2026, 13, null, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnMappedInvoices()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var fee1 = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Mensalidade Maio");
        fee1.ApplyPayment(100m, new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc));

        var fee2 = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Mensalidade Maio");

        _monthlyFeeRepo
            .Setup(x => x.GetInvoicesAsync(2026, 5, playerId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([fee1, fee2]);

        var result = await _handler.HandleAsync(new GetInvoicesQuery(2026, 5, playerId, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        var first = result.Value![0];
        first.Amount.Should().Be(100m);
        first.PaidAmount.Should().Be(100m);
        first.OpenAmount.Should().Be(0m);
        first.Status.Should().Be(MonthlyFeeStatus.Paid);

        var second = result.Value[1];
        second.OpenAmount.Should().Be(100m);
    }
}
