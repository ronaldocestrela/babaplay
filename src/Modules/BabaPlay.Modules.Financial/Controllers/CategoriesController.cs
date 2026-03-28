using BabaPlay.Modules.Financial.Entities;
using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Financial.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class CategoriesController : BaseController
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    public sealed record CategoryBody(string Name, CategoryType Type);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Name, body.Type, ct));
}
