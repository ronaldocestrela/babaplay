using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Financial.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CashEntriesController : BaseController
{
    private readonly CashEntryService _service;

    public CashEntriesController(CashEntryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    public sealed record CashEntryBody(decimal Amount, string CategoryId, string? Description, DateTime? EntryDate);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CashEntryBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Amount, body.CategoryId, body.Description, body.EntryDate, ct));
}
