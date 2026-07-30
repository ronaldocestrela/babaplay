using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Financial;

public sealed class CreateFundraiserCommandHandler : ICommandHandler<CreateFundraiserCommand, Result<FundraiserResponse>>
{
    private readonly IFundraiserRepository _fundraiserRepository;
    private readonly ITenantContext _tenantContext;

    public CreateFundraiserCommandHandler(
        IFundraiserRepository fundraiserRepository,
        ITenantContext tenantContext)
    {
        _fundraiserRepository = fundraiserRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<FundraiserResponse>> HandleAsync(CreateFundraiserCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<FundraiserResponse>.Fail("INVALID_TITLE", "Title is required.");

        if (command.TargetAmount <= 0)
            return Result<FundraiserResponse>.Fail("INVALID_TARGET", "Target amount must be greater than zero.");

        var tenantId = _tenantContext.TenantId;
        var fundraiser = Fundraiser.Create(
            tenantId,
            command.Title,
            command.Description,
            command.TargetAmount,
            command.DeadlineUtc);

        await _fundraiserRepository.AddAsync(fundraiser, ct);

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
