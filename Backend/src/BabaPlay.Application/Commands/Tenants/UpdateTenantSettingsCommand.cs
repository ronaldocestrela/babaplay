using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed record UpdateTenantSettingsCommand(
    Guid TenantId,
    string RequestedByUserId,
    string Name,
    TenantLogoUploadRequest? Logo,
    string Street,
    string Number,
    string? Neighborhood,
    string City,
    string State,
    string ZipCode)
    : ICommand<Result<TenantResponse>>;
