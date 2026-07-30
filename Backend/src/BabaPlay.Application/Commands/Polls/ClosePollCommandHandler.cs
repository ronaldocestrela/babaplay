using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Polls;

public sealed class ClosePollCommandHandler
    : ICommandHandler<ClosePollCommand, Result>
{
    private readonly IPollRepository _pollRepository;
    private readonly ITenantContext _tenantContext;

    public ClosePollCommandHandler(
        IPollRepository pollRepository,
        ITenantContext tenantContext)
    {
        _pollRepository = pollRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(
        ClosePollCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.PollId == Guid.Empty)
            return Result.Fail("INVALID_POLL", "PollId is required.");

        var tenantId = _tenantContext.TenantId;
        var poll = await _pollRepository.GetByIdAsync(command.PollId, tenantId, cancellationToken);

        if (poll is null)
            return Result.Fail("NOT_FOUND", "Poll not found or does not belong to the current tenant.");

        poll.Close();
        await _pollRepository.UpdateAsync(poll, cancellationToken);

        return Result.Ok();
    }
}
