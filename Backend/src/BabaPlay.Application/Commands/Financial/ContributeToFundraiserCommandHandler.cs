using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Financial;

public sealed class ContributeToFundraiserCommandHandler : ICommandHandler<ContributeToFundraiserCommand, Result<FundraiserResponse>>
{
    private readonly IFundraiserRepository _fundraiserRepository;
    private readonly ICashTransactionRepository _cashTransactionRepository;

    public ContributeToFundraiserCommandHandler(
        IFundraiserRepository fundraiserRepository,
        ICashTransactionRepository cashTransactionRepository)
    {
        _fundraiserRepository = fundraiserRepository;
        _cashTransactionRepository = cashTransactionRepository;
    }

    public async Task<Result<FundraiserResponse>> HandleAsync(ContributeToFundraiserCommand command, CancellationToken ct = default)
    {
        var fundraiser = await _fundraiserRepository.GetByIdAsync(command.FundraiserId, ct);
        if (fundraiser is null)
            return Result<FundraiserResponse>.Fail("FUNDRAISER_NOT_FOUND", "Fundraiser not found.");

        if (command.Amount <= 0)
            return Result<FundraiserResponse>.Fail("INVALID_AMOUNT", "Contribution amount must be greater than zero.");

        if (fundraiser.Status != FundraiserStatus.Active)
            return Result<FundraiserResponse>.Fail("FUNDRAISER_NOT_ACTIVE", "Fundraiser is not active for contributions.");

        var now = DateTime.UtcNow;
        fundraiser.AddContribution(command.Amount);

        var contributorInfo = !string.IsNullOrWhiteSpace(command.ContributorName) ? $" ({command.ContributorName})" : "";
        var cashTransaction = CashTransaction.Create(
            fundraiser.TenantId,
            CashTransactionType.Income,
            command.Amount,
            now,
            $"Contribuição Vaquinha: {fundraiser.Title}{contributorInfo}",
            command.PlayerId);

        await _fundraiserRepository.UpdateAsync(fundraiser, ct);
        await _cashTransactionRepository.AddAsync(cashTransaction, ct);

        var progress = fundraiser.TargetAmount > 0
            ? Math.Min(100.0, Math.Round((double)(fundraiser.CollectedAmount / fundraiser.TargetAmount) * 100, 1))
            : 0;

        var response = new FundraiserResponse(
            fundraiser.Id,
            fundraiser.TenantId,
            fundraiser.Title,
            fundraiser.Description,
            fundraiser.TargetAmount,
            fundraiser.CollectedAmount,
            progress,
            fundraiser.DeadlineUtc,
            fundraiser.Status,
            fundraiser.ContributionsCount,
            fundraiser.CreatedAt);

        return Result<FundraiserResponse>.Ok(response);
    }
}
