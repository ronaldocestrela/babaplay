using BabaPlay.Modules.TeamGeneration.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.TeamGeneration.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class TeamsController : BaseController
{
    private readonly TeamGenerationService _service;

    public TeamsController(TeamGenerationService service) => _service = service;

    public sealed record GenerateBody(string SessionId, int TeamCount = 2);

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateBody body, CancellationToken ct) =>
        FromResult(await _service.GenerateFromSessionAsync(body.SessionId, body.TeamCount, ct));

    [HttpGet("by-session/{sessionId}")]
    public async Task<IActionResult> BySession(string sessionId, CancellationToken ct) =>
        FromResult(await _service.GetWithMembersAsync(sessionId, ct));
}
