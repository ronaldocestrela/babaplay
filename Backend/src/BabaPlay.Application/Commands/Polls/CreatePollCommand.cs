using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Polls;

public sealed record CreatePollCommand(
    Guid AuthorId,
    string Title,
    string? Description,
    DateTime? ExpiresAtUtc,
    List<string> Options) : ICommand<Result<PollResponse>>;
