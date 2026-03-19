using BabaPlayShared.Library.Models.Responses.CheckIns.Response;
using BabaPlayShared.Library.Enum;
using BabaPlayShared.Library.Wrappers;
using MediatR;

namespace Application.Features.CheckIns.Queries;

public class GetTodayCheckInsQuery : IRequest<IResponseWrapper>
{
}

public class GetTodayCheckInsQueryHandler(ICheckInService checkInService) : IRequestHandler<GetTodayCheckInsQuery, IResponseWrapper>
{
    private readonly ICheckInService _checkInService = checkInService;

    public async Task<IResponseWrapper> Handle(GetTodayCheckInsQuery request, CancellationToken cancellationToken)
    {
        var checkIns = await _checkInService.GetCheckInsByDateAsync(DateTime.UtcNow, cancellationToken);
        if (checkIns is null || checkIns.Count == 0)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Nenhum check-in encontrado para hoje.");
        }

        var responses = checkIns.Select((c, index) => new CheckInResponse
        {
            Id = c.Id,
            AssociadoId = c.AssociadoId,
            FullName = c.Associado?.FullName ?? string.Empty,
            Positions = [.. c.Associado?.Position.Select(p => Enum.Parse<SoccerPosition>(p.ToString())) ?? new List<SoccerPosition>()],
            ArrivalOrder = index + 1,
            Date = c.Date,
            CheckInAtUtc = c.CheckInAtUtc
        }).ToList();

        return await ResponseWrapper<List<CheckInResponse>>.SuccessAsync(data: responses);
    }
}
