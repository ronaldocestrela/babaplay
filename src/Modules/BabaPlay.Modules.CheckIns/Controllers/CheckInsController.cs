using BabaPlay.Modules.CheckIns.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.CheckIns.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CheckInsController : BaseController
{
    private readonly CheckInService _service;

    public CheckInsController(CheckInService service) => _service = service;

    [HttpPost("sessions")]
    public async Task<IActionResult> StartSession(CancellationToken ct) =>
        FromResult(await _service.StartSessionAsync(GetUserId(), ct));

    public sealed record CheckInBody(string SessionId, string AssociateId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CheckInBody body, CancellationToken ct) =>
        FromResult(await _service.RegisterCheckInAsync(body.SessionId, body.AssociateId, ct));

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> ListSession(string sessionId, CancellationToken ct) =>
        FromResult(await _service.ListForSessionAsync(sessionId, ct));
}
