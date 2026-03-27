using BabaPlay.Modules.Platform.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Platform.Controllers;

[AllowAnonymous]
[Route("api/platform/[controller]")]
public sealed class PlansController : BaseController
{
    private readonly PlanService _service;

    public PlansController(PlanService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        FromResult(await _service.GetAsync(id, ct));

    public sealed record PlanBody(string Name, string? Description, decimal MonthlyPrice, int? MaxAssociates);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlanBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Name, body.Description, body.MonthlyPrice, body.MaxAssociates, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PlanBody body, CancellationToken ct) =>
        FromResult(await _service.UpdateAsync(id, body.Name, body.Description, body.MonthlyPrice, body.MaxAssociates, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct) =>
        FromResult(await _service.DeleteAsync(id, ct));
}
