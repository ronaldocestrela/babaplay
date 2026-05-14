using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class CreateCashTransactionCommandHandlerTests
{
    private readonly Mock<ICashTransactionRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateCashTransactionCommandHandler _handler;

    public CreateCashTransactionCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateCashTransactionCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_NonPositiveAmount_ShouldReturnValidationError()
    {
        var command = new CreateCashTransactionCommand(
            CashTransactionType.Income,
            0m,
            new DateTime(2026, 05, 01, 10, 0, 0, DateTimeKind.Utc),
            "Mensalidade",
            null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FINANCIAL_INVALID_AMOUNT");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistAndReturnResponse()
    {
        var command = new CreateCashTransactionCommand(
            CashTransactionType.Expense,
            80m,
            new DateTime(2026, 05, 02, 10, 0, 0, DateTimeKind.Utc),
            "Aluguel",
            Guid.NewGuid());

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Amount.Should().Be(80m);
        result.Value.Type.Should().Be(CashTransactionType.Expense);

        _repo.Verify(x => x.AddAsync(It.IsAny<CashTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
