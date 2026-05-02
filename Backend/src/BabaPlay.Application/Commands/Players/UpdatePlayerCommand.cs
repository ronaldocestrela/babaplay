using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

/// <summary>Command to update an existing player's profile within the current tenant.</summary>
public sealed record UpdatePlayerCommand(
    Guid PlayerId,
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth) : ICommand<Result<PlayerResponse>>;
