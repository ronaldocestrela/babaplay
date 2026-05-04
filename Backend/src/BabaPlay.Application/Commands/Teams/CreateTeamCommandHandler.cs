using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Teams;

public sealed class CreateTeamCommandHandler : ICommandHandler<CreateTeamCommand, Result<TeamResponse>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ITenantContext _tenantContext;

    public CreateTeamCommandHandler(ITeamRepository teamRepository, ITenantContext tenantContext)
    {
        _teamRepository = teamRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TeamResponse>> HandleAsync(CreateTeamCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<TeamResponse>.Fail("INVALID_NAME", "Team name is required.");

        if (cmd.MaxPlayers <= 0)
            return Result<TeamResponse>.Fail("INVALID_MAX_PLAYERS", "MaxPlayers must be greater than zero.");

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();
        var exists = await _teamRepository.ExistsByNormalizedNameAsync(normalizedName, ct);
        if (exists)
            return Result<TeamResponse>.Fail("TEAM_ALREADY_EXISTS", $"Team '{normalizedName}' already exists.");

        var team = Team.Create(_tenantContext.TenantId, cmd.Name, cmd.MaxPlayers);

        await _teamRepository.AddAsync(team, ct);
        await _teamRepository.SaveChangesAsync(ct);

        return Result<TeamResponse>.Ok(ToResponse(team));
    }

    private static TeamResponse ToResponse(Team team) => new(
        team.Id,
        team.TenantId,
        team.Name,
        team.MaxPlayers,
        team.IsActive,
        team.CreatedAt,
        team.PlayerIds.ToList());
}
