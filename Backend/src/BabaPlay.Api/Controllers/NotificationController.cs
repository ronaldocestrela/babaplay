using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Commands.Notifications;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>
/// F26 — Hub de Notificações no App.
/// GET /api/v1/notifications          → lista notificações do usuário e contagem de não lidas
/// PUT /api/v1/notifications/{id}/read → marca notificação individual como lida
/// PUT /api/v1/notifications/read-all  → marca todas as notificações do usuário como lidas
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class NotificationController : ControllerBase
{
    private readonly IQueryHandler<GetNotificationsQuery, Result<NotificationSummaryResponse>> _getNotificationsHandler;
    private readonly ICommandHandler<MarkNotificationReadCommand, Result> _markReadHandler;
    private readonly ICommandHandler<MarkAllNotificationsReadCommand, Result> _markAllReadHandler;

    public NotificationController(
        IQueryHandler<GetNotificationsQuery, Result<NotificationSummaryResponse>> getNotificationsHandler,
        ICommandHandler<MarkNotificationReadCommand, Result> markReadHandler,
        ICommandHandler<MarkAllNotificationsReadCommand, Result> markAllReadHandler)
    {
        _getNotificationsHandler = getNotificationsHandler;
        _markReadHandler = markReadHandler;
        _markAllReadHandler = markAllReadHandler;
    }

    /// <summary>Lists notifications for the authenticated user along with unread count.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool onlyUnread = false,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var query = new GetNotificationsQuery(userId, onlyUnread, limit);
        var result = await _getNotificationsHandler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return Ok(result.Value);
    }

    /// <summary>Marks a specific notification as read for the authenticated user.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid id, CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new MarkNotificationReadCommand(id, userId);
        var result = await _markReadHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { title = result.ErrorCode, detail = result.ErrorMessage });

            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);
        }

        return NoContent();
    }

    /// <summary>Marks all unread notifications as read for the authenticated user.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var command = new MarkAllNotificationsReadCommand(userId);
        var result = await _markAllReadHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return NoContent();
    }
}
