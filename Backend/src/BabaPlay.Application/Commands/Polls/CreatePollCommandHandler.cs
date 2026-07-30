using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Polls;

public sealed class CreatePollCommandHandler
    : ICommandHandler<CreatePollCommand, Result<PollResponse>>
{
    private readonly IPollRepository _pollRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public CreatePollCommandHandler(
        IPollRepository pollRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _pollRepository = pollRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PollResponse>> HandleAsync(
        CreatePollCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<PollResponse>.Fail("INVALID_TITLE", "Title is required.");

        if (command.AuthorId == Guid.Empty)
            return Result<PollResponse>.Fail("INVALID_AUTHOR", "AuthorId is required.");

        var validOptions = command.Options?
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        if (validOptions.Count < 2)
            return Result<PollResponse>.Fail("MINIMUM_OPTIONS_REQUIRED", "At least 2 distinct options are required to create a poll.");

        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
            return Result<PollResponse>.Fail("INVALID_TENANT", "Tenant context is not resolved.");

        var poll = Poll.Create(
            tenantId,
            command.AuthorId,
            command.Title,
            command.Description,
            command.ExpiresAtUtc);

        foreach (var optText in validOptions)
        {
            poll.AddOption(optText);
        }

        await _pollRepository.AddAsync(poll, cancellationToken);

        var author = await _userRepository.FindByIdAsync(command.AuthorId.ToString(), cancellationToken);
        var authorName = author?.Email ?? "Diretoria";

        var optionResponses = poll.Options
            .OrderBy(o => o.Order)
            .Select(o => new PollOptionResponse(o.Id, o.Text, o.Order, 0, 0.0))
            .ToList();

        var response = new PollResponse(
            poll.Id,
            poll.TenantId,
            poll.AuthorId,
            authorName,
            poll.Title,
            poll.Description,
            poll.ExpiresAtUtc,
            IsClosed: false,
            HasVoted: false,
            VotedOptionId: null,
            TotalVotes: 0,
            optionResponses,
            poll.CreatedAt);

        return Result<PollResponse>.Ok(response);
    }
}
