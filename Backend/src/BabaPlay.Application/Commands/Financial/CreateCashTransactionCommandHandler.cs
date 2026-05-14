using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Financial;

public sealed class CreateCashTransactionCommandHandler
    : ICommandHandler<CreateCashTransactionCommand, Result<CashTransactionResponse>>
{
    private readonly ICashTransactionRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateCashTransactionCommandHandler(
        ICashTransactionRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CashTransactionResponse>> HandleAsync(CreateCashTransactionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.Amount <= 0)
            return Result<CashTransactionResponse>.Fail("FINANCIAL_INVALID_AMOUNT", "Amount must be greater than zero.");

        if (cmd.OccurredOnUtc.Kind != DateTimeKind.Utc)
            return Result<CashTransactionResponse>.Fail("FINANCIAL_INVALID_OCCURRED_ON", "OccurredOnUtc must be UTC.");

        if (string.IsNullOrWhiteSpace(cmd.Description))
            return Result<CashTransactionResponse>.Fail("FINANCIAL_INVALID_DESCRIPTION", "Description is required.");

        if (_tenantContext.TenantId == Guid.Empty)
            return Result<CashTransactionResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        try
        {
            var transaction = CashTransaction.Create(
                _tenantContext.TenantId,
                cmd.Type,
                cmd.Amount,
                cmd.OccurredOnUtc,
                cmd.Description,
                cmd.PlayerId);

            await _repository.AddAsync(transaction, ct);
            await _repository.SaveChangesAsync(ct);

            return Result<CashTransactionResponse>.Ok(ToResponse(transaction));
        }
        catch (ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault();
            var message = firstError.Value?.FirstOrDefault() ?? ex.Message;
            return Result<CashTransactionResponse>.Fail("FINANCIAL_VALIDATION_ERROR", message);
        }
    }

    private static CashTransactionResponse ToResponse(CashTransaction transaction) => new(
        transaction.Id,
        transaction.TenantId,
        transaction.PlayerId,
        transaction.Type,
        transaction.Amount,
        transaction.SignedAmount,
        transaction.OccurredOnUtc,
        transaction.Description,
        transaction.IsActive,
        transaction.CreatedAt);
}
