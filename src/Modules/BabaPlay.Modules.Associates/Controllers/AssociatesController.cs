using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Associates.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AssociatesController : BaseController
{
    private readonly AssociateService _service;

    public AssociatesController(AssociateService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        FromResult(await _service.GetAsync(id, ct));

    public sealed record AssociateBody(string Name, string? Email, string? Phone, IReadOnlyList<string> PositionIds);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssociateBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Name, body.Email, body.Phone, body.PositionIds, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AssociateBody body, CancellationToken ct) =>
        FromResult(await _service.UpdateAsync(id, body.Name, body.Email, body.Phone, body.PositionIds, ct));

    public sealed record SetActiveBody(bool IsActive);

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> SetActive(string id, [FromBody] SetActiveBody body, CancellationToken ct) =>
        FromResult(await _service.SetActiveAsync(id, body.IsActive, ct));
}
