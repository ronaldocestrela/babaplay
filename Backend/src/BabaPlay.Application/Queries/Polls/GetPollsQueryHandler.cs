using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Polls;

public sealed class GetPollsQueryHandler
    : IQueryHandler<GetPollsQuery, Result<IReadOnlyList<PollResponse>>>
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public GetPollsQueryHandler(
        IPollRepository pollRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<PollResponse>>> HandleAsync(
        GetPollsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;

        var polls = await _pollRepository.GetByTenantAsync(
            tenantId,
            query.OnlyActive,
            cancellationToken);

        var responses = new List<PollResponse>(polls.Count);

        foreach (var poll in polls)
        {
            var author = await _userRepository.FindByIdAsync(poll.AuthorId.ToString(), cancellationToken);
            var authorName = author?.Email ?? "Diretoria";

            var totalVotes = poll.Votes.Count;
            var userVote = !string.IsNullOrWhiteSpace(query.RequestingUserId)
                ? poll.Votes.FirstOrDefault(v => v.UserId == query.RequestingUserId)
                : null;

            var hasVoted = userVote is not null;
            var votedOptionId = userVote?.OptionId;

            var options = poll.Options
                .OrderBy(o => o.Order)
                .Select(o =>
                {
                    var count = o.VotesCount;
                    var percentage = totalVotes > 0
                        ? Math.Round((double)count / totalVotes * 100, 1)
                        : 0.0;

                    return new PollOptionResponse(o.Id, o.Text, o.Order, count, percentage);
                })
                .ToList();

            var isClosed = poll.IsClosed || (poll.ExpiresAtUtc.HasValue && poll.ExpiresAtUtc.Value <= DateTime.UtcNow);

            responses.Add(new PollResponse(
                poll.Id,
                poll.TenantId,
                poll.AuthorId,
                authorName,
                poll.Title,
                poll.Description,
                poll.ExpiresAtUtc,
                isClosed,
                hasVoted,
                votedOptionId,
                totalVotes,
                options,
                poll.CreatedAt));
        }

        return Result<IReadOnlyList<PollResponse>>.Ok(responses);
    }
}
