using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.MatchSummaries;

public sealed class GenerateMatchSummaryCommandHandler
    : ICommandHandler<GenerateMatchSummaryCommand, Result<MatchSummaryResponse>>
{
    private readonly IMatchSummaryRepository _summaryRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchEventRepository _matchEventRepository;
    private readonly IMatchSummaryPdfGenerator _pdfGenerator;
    private readonly IMatchSummaryStorageService _storageService;
    private readonly ITenantContext _tenantContext;

    public GenerateMatchSummaryCommandHandler(
        IMatchSummaryRepository summaryRepository,
        IMatchRepository matchRepository,
        IMatchEventRepository matchEventRepository,
        IMatchSummaryPdfGenerator pdfGenerator,
        IMatchSummaryStorageService storageService,
        ITenantContext tenantContext)
    {
        _summaryRepository = summaryRepository;
        _matchRepository = matchRepository;
        _matchEventRepository = matchEventRepository;
        _pdfGenerator = pdfGenerator;
        _storageService = storageService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MatchSummaryResponse>> HandleAsync(GenerateMatchSummaryCommand cmd, CancellationToken ct = default)
    {
        if (cmd.MatchId == Guid.Empty)
            return Result<MatchSummaryResponse>.Fail("INVALID_MATCH_ID", "MatchId is required.");

        var match = await _matchRepository.GetByIdAsync(cmd.MatchId, ct);
        if (match is null)
            return Result<MatchSummaryResponse>.Fail("MATCH_NOT_FOUND", $"Match '{cmd.MatchId}' was not found.");

        if (match.Status != MatchStatus.Completed)
            return Result<MatchSummaryResponse>.Fail("MATCH_NOT_COMPLETED", "Match summary can only be generated for completed matches.");

        var existingSummary = await _summaryRepository.GetByMatchIdAsync(cmd.MatchId, ct);
        if (existingSummary is not null)
            return Result<MatchSummaryResponse>.Fail("MATCH_SUMMARY_ALREADY_EXISTS", "A summary already exists for this match.");

        var events = await _matchEventRepository.GetActiveByMatchAsync(cmd.MatchId, ct);
        var input = new MatchSummaryPdfInput(
            match.Id,
            match.GameDayId,
            match.HomeTeamId,
            match.AwayTeamId,
            match.Description,
            DateTime.UtcNow,
            events
                .OrderBy(e => e.Minute)
                .Select(e => new MatchSummaryPdfEventItem(e.TeamId, e.PlayerId, e.MatchEventTypeId, e.Minute, e.Notes))
                .ToList());

        var pdfBytes = await _pdfGenerator.GenerateAsync(input, ct);
        if (pdfBytes.Length == 0)
            return Result<MatchSummaryResponse>.Fail("MATCH_SUMMARY_GENERATION_FAILED", "PDF generation returned an empty file.");

        var storedFile = await _storageService.SaveAsync(
            new MatchSummaryFileSaveRequest(_tenantContext.TenantId, match.Id, pdfBytes),
            ct);

        var summary = MatchSummary.Create(
            _tenantContext.TenantId,
            match.Id,
            storedFile.StoragePath,
            storedFile.FileName,
            storedFile.ContentType,
            storedFile.SizeBytes);

        try
        {
            await _summaryRepository.AddAsync(summary, ct);
            await _summaryRepository.SaveChangesAsync(ct);
        }
        catch
        {
            _ = await _storageService.DeleteAsync(storedFile.StoragePath, ct);
            return Result<MatchSummaryResponse>.Fail(
                "MATCH_SUMMARY_PERSISTENCE_FAILED",
                "Failed to persist match summary metadata.");
        }

        return Result<MatchSummaryResponse>.Ok(ToResponse(summary));
    }

    private static MatchSummaryResponse ToResponse(MatchSummary summary) => new(
        summary.Id,
        summary.TenantId,
        summary.MatchId,
        summary.FileName,
        summary.ContentType,
        summary.SizeBytes,
        summary.GeneratedAtUtc,
        summary.IsActive);
}
