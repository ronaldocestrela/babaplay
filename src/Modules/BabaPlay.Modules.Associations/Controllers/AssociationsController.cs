using BabaPlay.Modules.Associations.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Associations.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AssociationsController : BaseController
{
    private readonly AssociationService _service;

    public AssociationsController(AssociationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        FromResult(await _service.GetAsync(id, ct));

    public sealed record UpsertBody(string? Id, string Name, string? Address, string? Regulation);

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertBody body, CancellationToken ct) =>
        FromResult(await _service.UpsertSingleAsync(body.Id, body.Name, body.Address, body.Regulation, ct));
}
