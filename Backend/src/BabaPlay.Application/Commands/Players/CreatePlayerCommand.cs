using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

/// <summary>Command to register a new player within the current tenant.</summary>
public sealed record CreatePlayerCommand(
    Guid UserId,
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth) : ICommand<Result<PlayerResponse>>;
