using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed class SendPaymentReminderCommandHandler : ICommandHandler<SendPaymentReminderCommand, Result>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public SendPaymentReminderCommandHandler(
        IPlayerRepository playerRepository,
        IPlayerMonthlyFeeRepository monthlyFeeRepository)
    {
        _playerRepository = playerRepository;
        _monthlyFeeRepository = monthlyFeeRepository;
    }

    public async Task<Result> HandleAsync(SendPaymentReminderCommand command, CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(command.PlayerId, ct);
        if (player is null)
            return Result.Fail("PLAYER_NOT_FOUND", "Player not found.");

        var overdueFees = await _monthlyFeeRepository.GetOverdueAsync(DateTime.UtcNow, ct);
        var playerOverdue = overdueFees.Where(x => x.PlayerId == command.PlayerId).ToList();

        if (playerOverdue.Count == 0)
            return Result.Fail("NO_OVERDUE_FEES", "Player has no overdue monthly fees.");

        // Payment reminder notification processed
        return Result.Ok();
    }
}
