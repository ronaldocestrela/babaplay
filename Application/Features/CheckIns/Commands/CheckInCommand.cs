using BabaPlayShared.Library.Enum;
using BabaPlayShared.Library.Models.Responses.CheckIns.Response;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.CheckIns.Commands;

public class CheckInCommand : IRequest<IResponseWrapper>
{
}

public class CheckInCommandHandler(
    ICheckInService checkInService,
    Identity.Users.ICurrentUserService currentUserService) : IRequestHandler<CheckInCommand, IResponseWrapper>
{
    private readonly ICheckInService _checkInService = checkInService;
    private readonly Identity.Users.ICurrentUserService _currentUserService = currentUserService;

    public async Task<IResponseWrapper> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return await ResponseWrapper<string>.FailAsync(message: "Usuário não autenticado.");
        }

        var checkIn = await _checkInService.CheckInAsync(userId, cancellationToken);

        var todayCheckIns = await _checkInService.GetCheckInsByDateAsync(checkIn.Date, cancellationToken);
        var arrivalOrder = todayCheckIns.FindIndex(c => c.Id == checkIn.Id) + 1;

        var response = new CheckInResponse
        {
            Id = checkIn.Id,
            AssociadoId = checkIn.AssociadoId,
            FullName = checkIn.Associado?.FullName ?? string.Empty,
            Positions = [.. checkIn.Associado?.Position.Select(p => Enum.Parse<SoccerPosition>(p.ToString())) ?? new List<SoccerPosition>()],
            ArrivalOrder = arrivalOrder,
            Date = checkIn.Date,
            CheckInAtUtc = checkIn.CheckInAtUtc
        };

        return await ResponseWrapper<CheckInResponse>.SuccessAsync(data: response, message: "Check-in registrado com sucesso.");
    }
}
