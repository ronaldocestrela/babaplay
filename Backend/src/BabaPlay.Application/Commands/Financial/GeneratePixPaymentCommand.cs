using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Financial;

public sealed record GeneratePixPaymentCommand(Guid InvoiceId) : ICommand<Result<PixPaymentDetailsResponse>>;
