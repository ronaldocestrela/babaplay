using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class ContributeToFundraiserCommandHandlerTests
{
    private readonly Mock<IFundraiserRepository> _fundraiserRepo = new();
    private readonly Mock<ICashTransactionRepository> _cashRepo = new();
    private readonly ContributeToFundraiserCommandHandler _handler;

    public ContributeToFundraiserCommandHandlerTests()
    {
        _handler = new ContributeToFundraiserCommandHandler(_fundraiserRepo.Object, _cashRepo.Object);
    }

    [Fact]
    public async Task Handle_FundraiserNotFound_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new ContributeToFundraiserCommand(Guid.NewGuid(), 50m));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FUNDRAISER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidContribution_ShouldAddContributionAndCashIncome()
    {
        var tenantId = Guid.NewGuid();
        var fundraiser = Fundraiser.Create(tenantId, "Bolas Novas", "Comprar 5 bolas", 500m);

        _fundraiserRepo.Setup(x => x.GetByIdAsync(fundraiser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fundraiser);

        var result = await _handler.HandleAsync(new ContributeToFundraiserCommand(fundraiser.Id, 250m, null, "Carlos"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CollectedAmount.Should().Be(250m);
        result.Value.ProgressPercentage.Should().Be(50.0);
        result.Value.ContributionsCount.Should().Be(1);

        _fundraiserRepo.Verify(x => x.UpdateAsync(fundraiser, It.IsAny<CancellationToken>()), Times.Once);
        _cashRepo.Verify(x => x.AddAsync(It.Is<CashTransaction>(c => c.Type == CashTransactionType.Income && c.Amount == 250m), It.IsAny<CancellationToken>()), Times.Once);
    }
}
