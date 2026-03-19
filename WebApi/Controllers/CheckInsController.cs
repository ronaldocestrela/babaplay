using Application.Features.CheckIns.Commands;
using Application.Features.CheckIns.Queries;
using BabaPlayShared.Library.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize]
public class CheckInsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CheckInAsync()
    {
        var response = await Sender.Send(new CheckInCommand());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayCheckInsAsync()
    {
        var response = await Sender.Send(new GetTodayCheckInsQuery());
        if (response.IsSuccessful)
        {
            return Ok(response);
        }

        return NotFound(response);
    }

    [HttpGet("teams")]
    public async Task<IActionResult> GetTeamAssignmentsAsync([FromQuery] DateTime? date = null)
    {
        var response = await Sender.Send(new GetTeamAssignmentsQuery { DateUtc = date });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }

        return NotFound(response);
    }
}

