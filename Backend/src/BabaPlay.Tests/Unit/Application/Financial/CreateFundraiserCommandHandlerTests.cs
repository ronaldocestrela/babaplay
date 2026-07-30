using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class CreateFundraiserCommandHandlerTests
{
    private readonly Mock<IFundraiserRepository> _fundraiserRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateFundraiserCommandHandler _handler;

    public CreateFundraiserCommandHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateFundraiserCommandHandler(_fundraiserRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_InvalidTitle_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new CreateFundraiserCommand("", "Descrição", 500m));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TITLE");
    }

    [Fact]
    public async Task Handle_InvalidTargetAmount_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new CreateFundraiserCommand("Churrasco", "Descrição", 0m));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TARGET");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateFundraiserAndReturnDto()
    {
        var command = new CreateFundraiserCommand("Churrasco Fim de Ano", "Arrecadação carnes e bebidas", 1200m);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Churrasco Fim de Ano");
        result.Value.TargetAmount.Should().Be(1200m);
        result.Value.CollectedAmount.Should().Be(0m);
        result.Value.ProgressPercentage.Should().Be(0);

        _fundraiserRepo.Verify(x => x.AddAsync(It.IsAny<Fundraiser>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
