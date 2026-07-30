using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Commands.Announcements;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Announcements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>
/// F24 — Mural de Avisos e Comunicados.
/// GET    /api/v1/communication/announcements           → lista comunicados do tenant
/// POST   /api/v1/communication/announcements           → cria comunicado (Admin/Manager)
/// POST   /api/v1/communication/announcements/{id}/mark-read → marca como lido
/// </summary>
[ApiController]
[Route("api/v1/communication/announcements")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class AnnouncementController : ControllerBase
{
    private readonly IQueryHandler<GetAnnouncementsQuery, Result<IReadOnlyList<AnnouncementResponse>>> _getAnnouncementsHandler;
    private readonly ICommandHandler<CreateAnnouncementCommand, Result<AnnouncementResponse>> _createAnnouncementHandler;
    private readonly ICommandHandler<MarkAnnouncementReadCommand, Result> _markReadHandler;

    public AnnouncementController(
        IQueryHandler<GetAnnouncementsQuery, Result<IReadOnlyList<AnnouncementResponse>>> getAnnouncementsHandler,
        ICommandHandler<CreateAnnouncementCommand, Result<AnnouncementResponse>> createAnnouncementHandler,
        ICommandHandler<MarkAnnouncementReadCommand, Result> markReadHandler)
    {
        _getAnnouncementsHandler = getAnnouncementsHandler;
        _createAnnouncementHandler = createAnnouncementHandler;
        _markReadHandler = markReadHandler;
    }

    /// <summary>Lists all published announcements for the current tenant, with per-user read status.</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationRead)]
    [ProducesResponseType(typeof(IReadOnlyList<AnnouncementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = new GetAnnouncementsQuery(OnlyPublished: true, RequestingUserId: userId);
        var result = await _getAnnouncementsHandler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return Ok(result.Value);
    }

    /// <summary>Creates and publishes a new announcement. Requires CommunicationWrite permission.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationWrite)]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var authorId))
            return Problem(detail: "Unable to resolve author from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new CreateAnnouncementCommand(
            authorId,
            request.Title,
            request.Content,
            request.ExpiresAtUtc,
            request.Tags);

        var result = await _createAnnouncementHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return CreatedAtAction(
            nameof(GetAnnouncements),
            null,
            result.Value);
    }

    /// <summary>Marks an announcement as read by the current user. Idempotent.</summary>
    [HttpPost("{id:guid}/mark-read")]
    [Authorize(Policy = AuthorizationPolicyNames.CommunicationRead)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new MarkAnnouncementReadCommand(id, userId);
        var result = await _markReadHandler.HandleAsync(command, ct);

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

/// <summary>Request body for creating a new announcement.</summary>
public sealed record CreateAnnouncementRequest(
    string Title,
    string Content,
    DateTime? ExpiresAtUtc = null,
    string? Tags = null);
