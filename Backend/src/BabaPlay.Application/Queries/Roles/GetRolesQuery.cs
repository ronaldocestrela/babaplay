using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Roles;

/// <summary>Query to list active roles from current tenant.</summary>
public sealed record GetRolesQuery() : IQuery<Result<IReadOnlyList<RoleResponse>>>;
