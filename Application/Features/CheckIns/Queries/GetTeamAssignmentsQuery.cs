using BabaPlayShared.Library.Models.Responses.CheckIns.Response;
using BabaPlayShared.Library.Enum;
using BabaPlayShared.Library.Wrappers;
using Domain.Entities;
using MediatR;

namespace Application.Features.CheckIns.Queries;

public class GetTeamAssignmentsQuery : IRequest<IResponseWrapper>
{
    public DateTime? DateUtc { get; set; }
}

public class GetTeamAssignmentsQueryHandler(ICheckInService checkInService) : IRequestHandler<GetTeamAssignmentsQuery, IResponseWrapper>
{
    private readonly ICheckInService _checkInService = checkInService;

    public async Task<IResponseWrapper> Handle(GetTeamAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var date = request.DateUtc?.Date ?? DateTime.UtcNow.Date;
        var (teamA, teamB) = await _checkInService.GetTeamsByDateAsync(date, cancellationToken);

        if ((teamA?.Count ?? 0) + (teamB?.Count ?? 0) == 0)
        {
            return await ResponseWrapper<string>.FailAsync(message: "Nenhum check-in encontrado para a data informada.");
        }

        static List<CheckInResponse> Map(IReadOnlyList<DailyCheckIn>? checks)
        {
            checks ??= [];
            return [.. checks.Select((c, index) => new CheckInResponse
            {
                Id = c.Id,
                AssociadoId = c.AssociadoId,
                FullName = c.Associado?.FullName ?? string.Empty,
                Positions = [.. c.Associado?.Position.Select(p => Enum.Parse<SoccerPosition>(p.ToString())) ?? new List<SoccerPosition>()],
                ArrivalOrder = index + 1,
                Date = c.Date,
                CheckInAtUtc = c.CheckInAtUtc
            })];
        }

        var response = new TeamAssignmentResponse
        {
            Date = date,
            TeamA = Map(teamA),
            TeamB = Map(teamB)
        };

        return await ResponseWrapper<TeamAssignmentResponse>.SuccessAsync(data: response);
    }
}
