using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record ConfirmPixPaymentCommand(Guid InvoiceId, string? TxId = null) : ICommand<Result<MonthlyFeePaymentResponse>>;
