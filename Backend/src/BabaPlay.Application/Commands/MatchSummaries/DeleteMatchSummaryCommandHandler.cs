using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchSummaries;

public sealed class DeleteMatchSummaryCommandHandler
    : ICommandHandler<DeleteMatchSummaryCommand, Result>
{
    private readonly IMatchSummaryRepository _summaryRepository;
    private readonly IMatchSummaryStorageService _storageService;

    public DeleteMatchSummaryCommandHandler(
        IMatchSummaryRepository summaryRepository,
        IMatchSummaryStorageService storageService)
    {
        _summaryRepository = summaryRepository;
        _storageService = storageService;
    }

    public async Task<Result> HandleAsync(DeleteMatchSummaryCommand cmd, CancellationToken ct = default)
    {
        if (cmd.SummaryId == Guid.Empty)
            return Result.Fail("INVALID_MATCH_SUMMARY_ID", "SummaryId is required.");

        var summary = await _summaryRepository.GetByIdAsync(cmd.SummaryId, ct);
        if (summary is null)
            return Result.Fail("MATCH_SUMMARY_NOT_FOUND", $"Match summary '{cmd.SummaryId}' was not found.");

        var deleted = await _storageService.DeleteAsync(summary.StoragePath, ct);
        if (!deleted)
            return Result.Fail("MATCH_SUMMARY_FILE_DELETE_FAILED", "Failed to delete summary file from storage.");

        summary.Deactivate();
        await _summaryRepository.UpdateAsync(summary, ct);

        return Result.Ok();
    }
}
