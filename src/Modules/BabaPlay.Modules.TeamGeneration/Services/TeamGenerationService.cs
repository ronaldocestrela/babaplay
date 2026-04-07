using BabaPlay.Modules.Associations.Entities;
using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.TeamGeneration.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.TeamGeneration.Services;

public sealed class TeamGenerationService
{
    private readonly ITenantRepository<Association> _associations;
    private readonly ITenantRepository<CheckIn> _checkIns;
    private readonly ITenantRepository<Team> _teams;
    private readonly ITenantRepository<TeamMember> _members;
    private readonly ITenantUnitOfWork _uow;

    public TeamGenerationService(
        ITenantRepository<Association> associations,
        ITenantRepository<CheckIn> checkIns,
        ITenantRepository<Team> teams,
        ITenantRepository<TeamMember> members,
        ITenantUnitOfWork uow)
    {
        _associations = associations;
        _checkIns = checkIns;
        _teams = teams;
        _members = members;
        _uow = uow;
    }

    /// <summary>
    /// Orders associates by first check-in time in the session, then assigns round-robin across teams.
    /// Team count is derived from the association's configured players-per-team and the number of checked-in associates.
    /// </summary>
    public async Task<Result<IReadOnlyList<Team>>> GenerateFromSessionAsync(string sessionId, CancellationToken ct)
    {
        var orderedAssociateIds = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var checkIns = await _checkIns.Query().Where(c => c.SessionId == sessionId).OrderBy(c => c.CheckedInAt)
            .ToListAsync(ct);
        foreach (var c in checkIns)
        {
            if (seen.Add(c.AssociateId))
                orderedAssociateIds.Add(c.AssociateId);
        }

        if (orderedAssociateIds.Count == 0)
            return Result.Invalid<IReadOnlyList<Team>>("No check-ins found for this session.");

        var association = await _associations.Query().FirstOrDefaultAsync(ct);
        var playersPerTeam = association?.PlayersPerTeam ?? 5;
        if (playersPerTeam < 2)
            return Result.Invalid<IReadOnlyList<Team>>("Players per team must be at least 2. Update association settings.");

        var teamCount = Math.Max(2, orderedAssociateIds.Count / playersPerTeam);

        var existingTeams = await _teams.Query().Where(t => t.SessionId == sessionId).ToListAsync(ct);
        foreach (var t in existingTeams)
        {
            var ms = await _members.Query().Where(m => m.TeamId == t.Id).ToListAsync(ct);
            foreach (var m in ms) _members.Remove(m);
            _teams.Remove(t);
        }

        await _uow.SaveChangesAsync(ct);

        var teams = new List<Team>();
        for (var i = 0; i < teamCount; i++)
        {
            var team = new Team { SessionId = sessionId, Name = $"Team {i + 1}" };
            await _teams.AddAsync(team, ct);
            teams.Add(team);
        }

        await _uow.SaveChangesAsync(ct);

        for (var idx = 0; idx < orderedAssociateIds.Count; idx++)
        {
            var target = teams[idx % teamCount];
            await _members.AddAsync(new TeamMember
            {
                TeamId = target.Id,
                AssociateId = orderedAssociateIds[idx],
                Order = idx
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success<IReadOnlyList<Team>>(teams);
    }

    public async Task<Result<IReadOnlyList<Team>>> GetWithMembersAsync(string sessionId, CancellationToken ct)
    {
        var list = await _teams.Query().Where(t => t.SessionId == sessionId).Include(t => t.Members).OrderBy(t => t.Name)
            .ToListAsync(ct);
        return Result.Success<IReadOnlyList<Team>>(list);
    }
}
