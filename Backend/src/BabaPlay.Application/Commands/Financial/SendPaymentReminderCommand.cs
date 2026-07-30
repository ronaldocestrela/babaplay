using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record SendPaymentReminderCommand(Guid PlayerId) : ICommand<Result>;
