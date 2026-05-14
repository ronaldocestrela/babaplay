using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed record SendAssociationInviteCommand(
    Guid TenantId,
    string RequestedByUserId,
    string Email,
    string AcceptLinkBaseUrl,
    int TokenExpiresInHours) : ICommand<Result<AssociationInviteResponse>>;
