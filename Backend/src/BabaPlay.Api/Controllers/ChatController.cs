using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BabaPlay.Application.Commands.Chat;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Chat;
using BabaPlay.Infrastructure.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Api.Controllers;

/// <summary>
/// F27 — Chat em Tempo Real via SignalR.
/// GET  /api/v1/communication/chat/messages → Retorna histórico recente de mensagens do tenant
/// POST /api/v1/communication/chat/messages → Persiste nova mensagem e transmite em tempo real via ChatHub SignalR
/// </summary>
[ApiController]
[Route("api/v1/communication/chat/messages")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class ChatController : ControllerBase
{
    private readonly IQueryHandler<GetRecentChatMessagesQuery, Result<IReadOnlyList<ChatMessageResponse>>> _getMessagesHandler;
    private readonly ICommandHandler<SendChatMessageCommand, Result<ChatMessageResponse>> _sendMessageHandler;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public ChatController(
        IQueryHandler<GetRecentChatMessagesQuery, Result<IReadOnlyList<ChatMessageResponse>>> getMessagesHandler,
        ICommandHandler<SendChatMessageCommand, Result<ChatMessageResponse>> sendMessageHandler,
        IHubContext<ChatHub> chatHubContext)
    {
        _getMessagesHandler = getMessagesHandler;
        _sendMessageHandler = sendMessageHandler;
        _chatHubContext = chatHubContext;
    }

    /// <summary>Lists recent chat messages for the association context in chronological order.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChatMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetRecentMessages([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var query = new GetRecentChatMessagesQuery(limit);
        var result = await _getMessagesHandler.HandleAsync(query, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        return Ok(result.Value);
    }

    /// <summary>Sends a chat message, persists it, and broadcasts to active SignalR clients.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request, CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Problem(detail: "Unable to resolve user from JWT.", statusCode: StatusCodes.Status400BadRequest, title: "INVALID_TOKEN");

        var userName = User.FindFirstValue("name")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? "Atleta";

        var command = new SendChatMessageCommand(userId, userName, request.Content);
        var result = await _sendMessageHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
            return Problem(detail: result.ErrorMessage, statusCode: StatusCodes.Status422UnprocessableEntity, title: result.ErrorCode);

        var messageDto = result.Value!;

        // Broadcast message to SignalR group for current tenant
        await _chatHubContext.Clients
            .Group(ChatHub.TenantGroup(messageDto.TenantId))
            .SendAsync(ChatHub.ReceiveMessageEvent, messageDto, cancellationToken: ct);

        return CreatedAtAction(nameof(GetRecentMessages), new { id = messageDto.Id }, messageDto);
    }
}
