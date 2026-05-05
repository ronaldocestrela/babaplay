using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class CreatePlayerMonthlyFeeCommandHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreatePlayerMonthlyFeeCommandHandler _handler;

    public CreatePlayerMonthlyFeeCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreatePlayerMonthlyFeeCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_ExistingMonthlyFee_ShouldReturnConflictError()
    {
        var playerId = Guid.NewGuid();
        var tenantId = _tenantContext.Object.TenantId;

        _repo
            .Setup(x => x.ExistsByPlayerAndCompetenceAsync(tenantId, playerId, 2026, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreatePlayerMonthlyFeeCommand(
            playerId,
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade maio");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MONTHLY_FEE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateMonthlyFee()
    {
        var playerId = Guid.NewGuid();
        var tenantId = _tenantContext.Object.TenantId;

        _repo
            .Setup(x => x.ExistsByPlayerAndCompetenceAsync(tenantId, playerId, 2026, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreatePlayerMonthlyFeeCommand(
            playerId,
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade maio");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PlayerId.Should().Be(playerId);
        _repo.Verify(x => x.AddAsync(It.IsAny<PlayerMonthlyFee>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
