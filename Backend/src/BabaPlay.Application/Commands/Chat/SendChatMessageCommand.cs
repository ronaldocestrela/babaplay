using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Chat;

public sealed record SendChatMessageCommand(
    Guid SenderId,
    string SenderName,
    string Content) : ICommand<Result<ChatMessageResponse>>;
