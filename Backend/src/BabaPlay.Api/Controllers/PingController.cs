using BabaPlay.Application.Commands.Ping;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Common;
using BabaPlay.Application.Queries.Ping;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class PingController : ControllerBase
{
    private readonly ICommandHandler<PingCommand, Result<string>> _pingCommandHandler;
    private readonly IQueryHandler<PingQuery, Result<PingStatusDto>> _pingQueryHandler;

    public PingController(
        ICommandHandler<PingCommand, Result<string>> pingCommandHandler,
        IQueryHandler<PingQuery, Result<PingStatusDto>> pingQueryHandler)
    {
        _pingCommandHandler = pingCommandHandler;
        _pingQueryHandler = pingQueryHandler;
    }

    /// <summary>
    /// Returns the current health status of the API (query — read-only).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PingStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var result = await _pingQueryHandler.HandleAsync(new PingQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Sends a ping command and receives a pong message (command — write intent).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Ping([FromBody] PingRequest request, CancellationToken cancellationToken)
    {
        var result = await _pingCommandHandler.HandleAsync(new PingCommand(request.Sender), cancellationToken);

        if (!result.IsSuccess)
            return UnprocessableEntity(new ProblemDetails { Title = result.ErrorCode, Detail = result.ErrorMessage });

        return Ok(result.Value);
    }
}

public record PingRequest(string Sender);
