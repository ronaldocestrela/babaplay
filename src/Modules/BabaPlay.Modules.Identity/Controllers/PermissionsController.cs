using BabaPlay.Modules.Identity.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Identity.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class PermissionsController : BaseController
{
    private readonly RoleAdminService _roles;

    public PermissionsController(RoleAdminService roles) => _roles = roles;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _roles.ListPermissionsAsync(ct));
}
