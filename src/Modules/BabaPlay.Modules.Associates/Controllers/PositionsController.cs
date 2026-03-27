using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Associates.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class PositionsController : BaseController
{
    private readonly PositionService _service;

    public PositionsController(PositionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    public sealed record PositionBody(string Name, int SortOrder);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PositionBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Name, body.SortOrder, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PositionBody body, CancellationToken ct) =>
        FromResult(await _service.UpdateAsync(id, body.Name, body.SortOrder, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct) =>
        FromResult(await _service.DeleteAsync(id, ct));
}
