using BabaPlay.Modules.Platform.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Platform.Controllers;

[AllowAnonymous]
[Route("api/platform/[controller]")]
public sealed class TenantsController : BaseController
{
    private readonly TenantSubscriptionService _service;

    public TenantsController(TenantSubscriptionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListTenantsAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        FromResult(await _service.GetTenantAsync(id, ct));

    public sealed record TenantBody(string Name, string Subdomain);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TenantBody body, CancellationToken ct) =>
        FromResult(await _service.CreateTenantAsync(body.Name, body.Subdomain, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] TenantBody body, CancellationToken ct) =>
        FromResult(await _service.UpdateTenantAsync(id, body.Name, body.Subdomain, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct) =>
        FromResult(await _service.DeleteTenantAsync(id, ct));

    public sealed record SubscribeBody(string PlanId);

    [HttpPost("{id}/subscription")]
    public async Task<IActionResult> Subscribe(string id, [FromBody] SubscribeBody body, CancellationToken ct) =>
        FromResult(await _service.SubscribeTenantAsync(id, body.PlanId, ct));
}
