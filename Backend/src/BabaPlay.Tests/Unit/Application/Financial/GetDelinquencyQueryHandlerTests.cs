using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetDelinquencyQueryHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _repo = new();
    private readonly GetDelinquencyQueryHandler _handler;

    public GetDelinquencyQueryHandlerTests()
    {
        _handler = new GetDelinquencyQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_NonUtcReference_ShouldReturnValidationError()
    {
        var reference = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Local);

        var result = await _handler.HandleAsync(new GetDelinquencyQuery(reference));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_ValidReference_ShouldReturnOpenAmounts()
    {
        var tenantId = Guid.NewGuid();
        var reference = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);

        var fee1 = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 4, 120m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), "Abril");
        fee1.ApplyPayment(20m, new DateTime(2026, 4, 12, 0, 0, 0, DateTimeKind.Utc));

        var fee2 = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 4, 100m, new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), "Abril");

        _repo.Setup(x => x.GetOverdueAsync(reference, It.IsAny<CancellationToken>())).ReturnsAsync([fee1, fee2]);

        var result = await _handler.HandleAsync(new GetDelinquencyQuery(reference));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalOpenAmount.Should().Be(200m);
        result.Value.Items.Should().HaveCount(2);
    }
}
