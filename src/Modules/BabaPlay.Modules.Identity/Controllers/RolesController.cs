using BabaPlay.Modules.Identity.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Identity.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class RolesController : BaseController
{
    private readonly RoleAdminService _roles;

    public RolesController(RoleAdminService roles) => _roles = roles;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _roles.ListRolesAsync(ct));

    [HttpPost("users/{userId}/assign/{roleName}")]
    public async Task<IActionResult> Assign(string userId, string roleName, CancellationToken ct) =>
        FromResult(await _roles.AssignRoleAsync(userId, roleName, ct));
}
