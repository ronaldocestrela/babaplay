using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Polls;

public sealed class SubmitPollVoteCommandHandler
    : ICommandHandler<SubmitPollVoteCommand, Result<PollResponse>>
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public SubmitPollVoteCommandHandler(
        IPollRepository pollRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PollResponse>> HandleAsync(
        SubmitPollVoteCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PollId == Guid.Empty)
            return Result<PollResponse>.Fail("INVALID_POLL", "PollId is required.");

        if (command.OptionId == Guid.Empty)
            return Result<PollResponse>.Fail("INVALID_OPTION", "OptionId is required.");

        if (string.IsNullOrWhiteSpace(command.UserId))
            return Result<PollResponse>.Fail("INVALID_USER", "UserId is required.");

        var tenantId = _tenantContext.TenantId;
        var poll = await _pollRepository.GetByIdAsync(command.PollId, tenantId, cancellationToken);

        if (poll is null)
            return Result<PollResponse>.Fail("NOT_FOUND", "Poll not found or does not belong to the current tenant.");

        if (poll.IsClosed || (poll.ExpiresAtUtc.HasValue && poll.ExpiresAtUtc.Value <= DateTime.UtcNow))
            return Result<PollResponse>.Fail("POLL_CLOSED", "This poll is closed for voting.");

        var alreadyVoted = await _pollRepository.HasUserVotedAsync(command.PollId, command.UserId, cancellationToken);
        if (alreadyVoted)
            return Result<PollResponse>.Fail("ALREADY_VOTED", "User has already voted in this poll.");

        try
        {
            var vote = poll.Vote(command.OptionId, command.UserId);
            await _pollRepository.AddVoteAsync(vote, cancellationToken);
            await _pollRepository.UpdateAsync(poll, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<PollResponse>.Fail("VOTE_FAILED", ex.Message);
        }

        var author = await _userRepository.FindByIdAsync(poll.AuthorId.ToString(), cancellationToken);
        var authorName = author?.Email ?? "Diretoria";

        var totalVotes = poll.Votes.Count;
        var optionResponses = poll.Options
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

        var response = new PollResponse(
            poll.Id,
            poll.TenantId,
            poll.AuthorId,
            authorName,
            poll.Title,
            poll.Description,
            poll.ExpiresAtUtc,
            poll.IsClosed,
            HasVoted: true,
            VotedOptionId: command.OptionId,
            TotalVotes: totalVotes,
            optionResponses,
            poll.CreatedAt);

        return Result<PollResponse>.Ok(response);
    }
}
