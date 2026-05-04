using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchSummaries;

public sealed class GetMatchSummaryFileQueryHandler
    : IQueryHandler<GetMatchSummaryFileQuery, Result<MatchSummaryFileResponse>>
{
    private readonly IMatchSummaryRepository _summaryRepository;
    private readonly IMatchSummaryStorageService _storageService;

    public GetMatchSummaryFileQueryHandler(
        IMatchSummaryRepository summaryRepository,
        IMatchSummaryStorageService storageService)
    {
        _summaryRepository = summaryRepository;
        _storageService = storageService;
    }

    public async Task<Result<MatchSummaryFileResponse>> HandleAsync(GetMatchSummaryFileQuery query, CancellationToken ct = default)
    {
        var summary = await _summaryRepository.GetByIdAsync(query.SummaryId, ct);
        if (summary is null)
            return Result<MatchSummaryFileResponse>.Fail("MATCH_SUMMARY_NOT_FOUND", $"Match summary '{query.SummaryId}' was not found.");

        var content = await _storageService.ReadAsync(summary.StoragePath, ct);
        if (content is null)
            return Result<MatchSummaryFileResponse>.Fail("MATCH_SUMMARY_FILE_NOT_FOUND", "Summary file was not found in storage.");

        return Result<MatchSummaryFileResponse>.Ok(new MatchSummaryFileResponse(summary.FileName, summary.ContentType, content));
    }
}
