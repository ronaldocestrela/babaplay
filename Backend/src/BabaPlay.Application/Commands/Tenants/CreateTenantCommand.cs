using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

/// <summary>Initiates tenant creation and enqueues async database provisioning.</summary>
public sealed record CreateTenantCommand(
	string Name,
	string Slug,
	string? AdminEmail,
	string? AdminPassword,
	string? RequestedByUserId)
	: ICommand<Result<TenantResponse>>;
