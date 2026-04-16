using BabaPlay.Modules.MatchReports.Dtos;
using BabaPlay.Modules.MatchReports.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.MatchReports.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class MatchReportsController : BaseController
{
    private readonly MatchReportService _service;

    public MatchReportsController(MatchReportService service) => _service = service;

    public sealed record MatchReportPlayerStatBody(
        string AssociateId,
        int Goals,
        int Assists,
        int YellowCards,
        int RedCards,
        string? Observations);

    public sealed record MatchReportGameBody(
        string Title,
        string? Notes,
        IReadOnlyList<MatchReportPlayerStatBody>? PlayerStats);

    public sealed record UpsertMatchReportBody(
        string? Notes,
        IReadOnlyList<MatchReportGameBody>? Games);

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetBySession(string sessionId, CancellationToken ct) =>
        FromResult(await _service.GetBySessionAsync(sessionId, ct));

    [HttpPut("sessions/{sessionId}")]
    public async Task<IActionResult> Upsert(string sessionId, [FromBody] UpsertMatchReportBody body, CancellationToken ct)
    {
        var gameInputs = (body.Games ?? Array.Empty<MatchReportGameBody>())
            .Select(game => new MatchReportGameInput(
                game.Title,
                game.Notes,
                (game.PlayerStats ?? Array.Empty<MatchReportPlayerStatBody>())
                    .Select(stat => new MatchReportPlayerStatInput(
                        stat.AssociateId,
                        stat.Goals,
                        stat.Assists,
                        stat.YellowCards,
                        stat.RedCards,
                        stat.Observations))
                    .ToList()))
            .ToList();

        return FromResult(await _service.UpsertAsync(
            sessionId,
            body.Notes,
            gameInputs,
            GetUserId(),
            User.IsInRole("Admin"),
            ct));
    }

    [HttpPost("sessions/{sessionId}/finalize")]
    public async Task<IActionResult> Finalize(string sessionId, CancellationToken ct) =>
        FromResult(await _service.FinalizeAsync(sessionId, GetUserId(), ct));
}