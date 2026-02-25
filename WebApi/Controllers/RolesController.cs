using Application.Features.Identity.Roles.Commands;
using BabaPlayShared.Library.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Identity.Roles.Queries;
using BabaPlayShared.Library.Models.Requests.Identity;


namespace WebApi.Controllers;

public class RolesController : BaseApiController
{
    [HttpPost("add")]
    [ShouldHavePermission(AssociationAction.Create, AssociationFeature.Roles)]
    public async Task<IActionResult> AddRoleAsync([FromBody] CreateRoleRequest createRole)
    {
        var response = await Sender.Send(new CreateRoleCommand { CreateRole = createRole });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("update")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Roles)]
    public async Task<IActionResult> UpdateRoleAsync([FromBody] UpdateRoleRequest updateRole)
    {
        var response = await Sender.Send(new UpdateRoleCommand { UpdateRole = updateRole });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("update-permissions")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.RoleClaims)]
    public async Task<IActionResult> UpdateRoleClaimsAsync([FromBody] UpdateRolePermissionsRequest updateRoleClaims)
    {
        var response = await Sender.Send(new UpdateRolePermissionsCommand { UpdateRolePermissions = updateRoleClaims });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpDelete("delete/{roleId}")]
    [ShouldHavePermission(AssociationAction.Delete, AssociationFeature.Roles)]
    public async Task<IActionResult> DeleteRoleAsync(string roleId)
    {
        var response = await Sender.Send(new DeleteRoleCommand { RoleId = roleId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Roles)]
    public async Task<IActionResult> GetRolesAsync()
    {
        var response = await Sender.Send(new GetRolesQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("partial/{roleId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Roles)]
    public async Task<IActionResult> GetPartialRoleByIdAsync(string roleId)
    {
        var response = await Sender.Send(new GetRoleByIdQuery { RoleId = roleId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("full/{roleId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Roles)]
    public async Task<IActionResult> GetDetailedRoleByIdAsync(string roleId)
    {
        var response = await Sender.Send(new GetRoleWithPermissionsQuery { RoleId = roleId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
