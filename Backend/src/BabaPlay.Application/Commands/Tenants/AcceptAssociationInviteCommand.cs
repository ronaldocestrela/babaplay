using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed record AcceptAssociationInviteCommand(
    string Token,
    string UserId) : ICommand<Result<AssociationInviteAcceptResponse>>;

public sealed record RegisterAndAcceptAssociationInviteCommand(
    string Token,
    string Email,
    string Password) : ICommand<Result<AssociationInviteAcceptResponse>>;
