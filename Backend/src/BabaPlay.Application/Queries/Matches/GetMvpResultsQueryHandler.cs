using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Matches;

public sealed class GetMvpResultsQueryHandler
    : IQueryHandler<GetMvpResultsQuery, Result<MvpResultApplicationDto>>
{
    private readonly IMatchRepository _matchRepository;

    public GetMvpResultsQueryHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result<MvpResultApplicationDto>> HandleAsync(
        GetMvpResultsQuery query,
        CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(query.MatchId, ct);

        var candidates = new List<MvpCandidateApplicationDto>
        {
            new(Guid.NewGuid(), "Zico Santos", null, "Meia", "Time Amarelo 🟡", 2, 1, 8, 40.0, false),
            new(Guid.NewGuid(), "Romário Farias", null, "Atacante", "Time Azul 🔵", 1, 2, 6, 30.0, false),
            new(Guid.NewGuid(), "Pelé Nascimento", null, "Atacante", "Time Amarelo 🟡", 1, 0, 4, 20.0, false),
            new(Guid.NewGuid(), "Goleiro Bruno", null, "Goleiro", "Time Azul 🔵", 0, 0, 2, 10.0, false)
        };

        var result = new MvpResultApplicationDto(
            query.MatchId,
            20,
            true,
            null,
            "Zico Santos",
            candidates
        );

        return Result<MvpResultApplicationDto>.Ok(result);
    }
}
