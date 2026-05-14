using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Financial;

public sealed class CreatePlayerMonthlyFeeCommandHandler
    : ICommandHandler<CreatePlayerMonthlyFeeCommand, Result<PlayerMonthlyFeeResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreatePlayerMonthlyFeeCommandHandler(
        IPlayerMonthlyFeeRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PlayerMonthlyFeeResponse>> HandleAsync(CreatePlayerMonthlyFeeCommand cmd, CancellationToken ct = default)
    {
        if (cmd.PlayerId == Guid.Empty)
            return Result<PlayerMonthlyFeeResponse>.Fail("FINANCIAL_INVALID_PLAYER_ID", "PlayerId is required.");

        if (cmd.Amount <= 0)
            return Result<PlayerMonthlyFeeResponse>.Fail("FINANCIAL_INVALID_AMOUNT", "Amount must be greater than zero.");

        if (cmd.DueDateUtc.Kind != DateTimeKind.Utc)
            return Result<PlayerMonthlyFeeResponse>.Fail("FINANCIAL_INVALID_DUE_DATE", "DueDateUtc must be UTC.");

        if (_tenantContext.TenantId == Guid.Empty)
            return Result<PlayerMonthlyFeeResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        var alreadyExists = await _repository.ExistsByPlayerAndCompetenceAsync(
            _tenantContext.TenantId,
            cmd.PlayerId,
            cmd.Year,
            cmd.Month,
            ct);

        if (alreadyExists)
            return Result<PlayerMonthlyFeeResponse>.Fail("MONTHLY_FEE_ALREADY_EXISTS", "Monthly fee already exists for this competence.");

        try
        {
            var monthlyFee = PlayerMonthlyFee.Create(
                _tenantContext.TenantId,
                cmd.PlayerId,
                cmd.Year,
                cmd.Month,
                cmd.Amount,
                cmd.DueDateUtc,
                cmd.Description);

            await _repository.AddAsync(monthlyFee, ct);
            await _repository.SaveChangesAsync(ct);

            return Result<PlayerMonthlyFeeResponse>.Ok(ToResponse(monthlyFee));
        }
        catch (ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault();
            var message = firstError.Value?.FirstOrDefault() ?? ex.Message;
            return Result<PlayerMonthlyFeeResponse>.Fail("FINANCIAL_VALIDATION_ERROR", message);
        }
    }

    private static PlayerMonthlyFeeResponse ToResponse(PlayerMonthlyFee monthlyFee) => new(
        monthlyFee.Id,
        monthlyFee.TenantId,
        monthlyFee.PlayerId,
        monthlyFee.Year,
        monthlyFee.Month,
        monthlyFee.Amount,
        monthlyFee.PaidAmount,
        monthlyFee.DueDateUtc,
        monthlyFee.PaidAtUtc,
        monthlyFee.Status,
        monthlyFee.Description,
        monthlyFee.IsActive,
        monthlyFee.CreatedAt);
}
