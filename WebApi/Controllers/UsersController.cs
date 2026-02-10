using Application.Features.Identity.Users;
using Application.Features.Identity.Users.Commands;
using Application.Features.Identity.Users.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class UsersController : BaseApiController
{
    [HttpPost("register")]
    [ShouldHavePermission(AssociationAction.Create, AssociationFeature.Users)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] CreateUserRequest createUser)
    {
        var response = await Sender.Send(new CreateUserCommand { CreateUser = createUser });

        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPut("update")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Users)]
    public async Task<IActionResult> UpdateUserDetailsAsync([FromBody] UpdateUserRequest updateUser)
    {
        var response = await Sender.Send(new UpdateUserCommand { UpdateUser = updateUser });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpPut("update-status")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.Users)]
    public async Task<IActionResult> ChangeUserStatusAsync([FromBody] ChangeUserStatusRequest changeUserStatus)
    {
        var response = await Sender.Send(new UpdateUserStatusCommand { ChangeUserStatus = changeUserStatus });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpPut("update-roles/{userId}")]
    [ShouldHavePermission(AssociationAction.Update, AssociationFeature.UserRoles)]
    public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UserRolesRequest userRolesRequest, string userId)
    {
        var response = await Sender.Send(new UpdateUserRolesCommand { UserRolesRequest = userRolesRequest, UserId = userId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpDelete("delete/{userId}")]
    [ShouldHavePermission(AssociationAction.Delete, AssociationFeature.Users)]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        var response = await Sender.Send(new DeleteUserCommand { UserId = userId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Users)]
    public async Task<IActionResult> GetUsersAsync()
    {
        var response = await Sender.Send(new GetAllUsersQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("{userId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.Users)]
    public async Task<IActionResult> GetUserByIdAsync(string userId)
    {
        var response = await Sender.Send(new GetUserByIdQuery { UserId = userId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("permissions/{userId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.RoleClaims)]
    public async Task<IActionResult> GetUserPermissionsAsync(string userId)
    {
        var response = await Sender.Send(new GetUserPermissionsQuery { UserId = userId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }

    [HttpGet("user-roles/{userId}")]
    [ShouldHavePermission(AssociationAction.Read, AssociationFeature.UserRoles)]
    public async Task<IActionResult> GetUserRolesAsync(string userId)
    {
        var response = await Sender.Send(new GetUserRolesQuery { UserId = userId });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return NotFound(response);
    }
}
