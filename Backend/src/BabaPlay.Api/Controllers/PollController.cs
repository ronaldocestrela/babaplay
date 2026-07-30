using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Commands.Polls;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Polls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>
/// F25 — Enquetes Interativas.
/// GET    /api/v1/communication/polls           → lista enquetes da associação
/// POST   /api/v1/communication/polls           → cria nova enquete com opções (Admin/Manager)
/// POST   /api/v1/communication/polls/{id}/vote → registra voto do usuário
/// POST   /api/v1/communication/polls/{id}/close → encerra enquete (Admin/Manager)
/// </summary>
[ApiController]
[Route("api/v1/communication/polls")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class PollController : ControllerBase
{
    private readonly IQueryHandler<GetPollsQuery, Result<IReadOnlyList<PollResponse>>> _getPollsHandler;
    private readonly ICommandHandler<CreatePollCommand, Result<PollResponse>> _createPollHandler;
    private readonly ICommandHandler<SubmitPollVoteCommand, Result<PollResponse>> _submitVoteHandler;
    private readonly ICommandHandler<ClosePollCommand, Result> _closePollHandler;

    public PollController(
        IQueryHandler<GetPollsQuery, Result<IReadOnlyList<PollResponse>>> getPollsHandler,
        ICommandHandler<CreatePollCommand, Result<PollResponse>> createPollHandler,
        ICommandHandler<SubmitPollVoteCommand, Result<PollResponse>> submitVoteHandler,
        ICommandHandler<ClosePollCommand, Result> closePollHandler)
    {
        _getPollsHandler = getPollsHandler;
        _createPollHandler = createPollHandler;
        _submitVoteHandler = submitVoteHandler;
        _closePollHandler = closePollHandler;
    }

    /// <summary>Lists all polls for the current tenant with user vote status and option breakdown.</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationRead)]
    [ProducesResponseType(typeof(IReadOnlyList<PollResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPolls([FromQuery] bool onlyActive = true, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = new GetPollsQuery(OnlyActive: onlyActive, RequestingUserId: userId);
        var result = await _getPollsHandler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return Ok(result.Value);
    }

    /// <summary>Creates a new interactive poll with choices. Requires CommunicationWrite permission.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationWrite)]
    [ProducesResponseType(typeof(PollResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var authorId))
            return Problem(detail: "Unable to resolve author from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new CreatePollCommand(
            authorId,
            request.Title,
            request.Description,
            request.ExpiresAtUtc,
            request.Options);

        var result = await _createPollHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return CreatedAtAction(nameof(GetPolls), null, result.Value);
    }

    /// <summary>Submits the current user's vote for an option in the specified poll.</summary>
    [HttpPost("{id:guid}/vote")]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationRead)]
    [ProducesResponseType(typeof(PollResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitVote([FromRoute] Guid id, [FromBody] SubmitVoteRequest request, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new SubmitPollVoteCommand(id, request.OptionId, userId);
        var result = await _submitVoteHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { title = result.ErrorCode, detail = result.ErrorMessage });

            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);
        }

        return Ok(result.Value);
    }

    /// <summary>Manually closes an interactive poll. Requires CommunicationWrite permission.</summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ClosePoll([FromRoute] Guid id, CancellationToken ct = default)
    {
        var command = new ClosePollCommand(id);
        var result = await _closePollHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { title = result.ErrorCode, detail = result.ErrorMessage });

            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);
        }

        return NoContent();
    }
}

// ─── Request Models ───────────────────────────────────────────────────────────

public sealed record CreatePollRequest(
    string Title,
    string? Description,
    DateTime? ExpiresAtUtc,
    List<string> Options);

public sealed record SubmitVoteRequest(Guid OptionId);
