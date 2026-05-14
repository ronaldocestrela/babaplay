using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed class DeleteMatchCommandHandler
    : ICommandHandler<DeleteMatchCommand, Result>
{
    private readonly IMatchRepository _matchRepository;

    public DeleteMatchCommandHandler(IMatchRepository matchRepository)
        => _matchRepository = matchRepository;

    public async Task<Result> HandleAsync(DeleteMatchCommand cmd, CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(cmd.MatchId, ct);
        if (match is null)
            return Result.Fail("MATCH_NOT_FOUND", $"Match '{cmd.MatchId}' was not found.");

        match.Deactivate();
        await _matchRepository.UpdateAsync(match, ct);
        await _matchRepository.SaveChangesAsync(ct);
        return Result.Ok();
    }
}