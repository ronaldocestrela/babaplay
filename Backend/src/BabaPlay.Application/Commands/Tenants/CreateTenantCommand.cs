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
	string? RequestedByUserId,
	TenantLogoUploadRequest? Logo,
	string Street,
	string Number,
	string? Neighborhood,
	string City,
	string State,
	string ZipCode,
	double AssociationLatitude,
	double AssociationLongitude)
	: ICommand<Result<TenantResponse>>;

public sealed record TenantLogoUploadRequest(
	string FileName,
	string ContentType,
	byte[] Content);
