using BabaPlay.Application.Queries.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetFundraisersQueryHandlerTests
{
    private readonly Mock<IFundraiserRepository> _fundraiserRepo = new();
    private readonly GetFundraisersQueryHandler _handler;

    public GetFundraisersQueryHandlerTests()
    {
        _handler = new GetFundraisersQueryHandler(_fundraiserRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFundraisersListWithProgress()
    {
        var tenantId = Guid.NewGuid();
        var f1 = Fundraiser.Create(tenantId, "Churrasco Fim de Ano", "Carnes e bebidas", 1000m);
        f1.AddContribution(500m);

        var f2 = Fundraiser.Create(tenantId, "Colete Novo", "Comprar 20 coletes", 300m);

        _fundraiserRepo.Setup(x => x.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Fundraiser> { f1, f2 });

        var result = await _handler.HandleAsync(new GetFundraisersQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        result.Value![0].Title.Should().Be("Churrasco Fim de Ano");
        result.Value[0].ProgressPercentage.Should().Be(50.0);

        result.Value[1].Title.Should().Be("Colete Novo");
        result.Value[1].ProgressPercentage.Should().Be(0.0);
    }
}
