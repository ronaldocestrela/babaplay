using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Players;

/// <summary>Command to manually register a player by creating account and membership in the current tenant.</summary>
public sealed record RegisterManualPlayerCommand(
    string Email,
    string Password,
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth) : ICommand<Result<PlayerResponse>>;
