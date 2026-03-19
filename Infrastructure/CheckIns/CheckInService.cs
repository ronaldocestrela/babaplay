using Application.Exceptions;
using Application.Features.CheckIns;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.CheckIns;

public class CheckInService(ApplicationDbContext context, ILogger<CheckInService> logger) : ICheckInService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<CheckInService> _logger = logger;

    public async Task<DailyCheckIn> CheckInAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException(["Usuário não autenticado."]);
        }

        var associado = await _context.Associados
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);

        if (associado is null)
        {
            throw new NotFoundException(["Associado não encontrado."]);
        }

        var today = DateTime.UtcNow.Date;

        if (await _context.DailyCheckIns
            .AnyAsync(c => c.AssociadoId == associado.Id && c.Date == today, ct))
        {
            throw new ConflictException(["Você já realizou check-in hoje."]);
        }

        var checkIn = new DailyCheckIn
        {
            AssociadoId = associado.Id,
            Date = today,
            CheckInAtUtc = DateTime.UtcNow
        };

        await _context.DailyCheckIns.AddAsync(checkIn, ct);
        await _context.SaveChangesAsync(ct);

        return checkIn;
    }

    public async Task<List<DailyCheckIn>> GetCheckInsByDateAsync(DateTime dateUtc, CancellationToken ct = default)
    {
        var date = dateUtc.Date;
        return await _context.DailyCheckIns
            .Include(c => c.Associado)
            .Where(c => c.Date == date)
            .OrderBy(c => c.CheckInAtUtc)
            .ThenBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<(List<DailyCheckIn> TeamA, List<DailyCheckIn> TeamB)> GetTeamsByDateAsync(DateTime dateUtc, CancellationToken ct = default)
    {
        var checkIns = await GetCheckInsByDateAsync(dateUtc, ct);

        // Basic balancing: distribute by position group while preserving arrival order.
        var teamA = new List<DailyCheckIn>();
        var teamB = new List<DailyCheckIn>();
        var teamCountsA = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var teamCountsB = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var preferA = false;

        static string GetPositionGroup(DailyCheckIn checkIn)
        {
            var pos = checkIn.Associado?.Position?.FirstOrDefault();
            return pos switch
            {
                Domain.Enums.SoccerPosition.GK => "Goalkeeper",
                Domain.Enums.SoccerPosition.CB or Domain.Enums.SoccerPosition.LB or Domain.Enums.SoccerPosition.RB or Domain.Enums.SoccerPosition.LWB or Domain.Enums.SoccerPosition.RWB => "Defense",
                Domain.Enums.SoccerPosition.CM or Domain.Enums.SoccerPosition.CDM or Domain.Enums.SoccerPosition.CAM or Domain.Enums.SoccerPosition.LM or Domain.Enums.SoccerPosition.RM => "Midfield",
                Domain.Enums.SoccerPosition.ST or Domain.Enums.SoccerPosition.LW or Domain.Enums.SoccerPosition.RW => "Attack",
                _ => "Unknown",
            };
        }

        foreach (var checkIn in checkIns)
        {
            var group = GetPositionGroup(checkIn);
            teamCountsA.TryGetValue(group, out var aCount);
            teamCountsB.TryGetValue(group, out var bCount);

            bool assignToA;
            if (aCount < bCount) assignToA = true;
            else if (bCount < aCount) assignToA = false;
            else
            {
                assignToA = !preferA;
                preferA = !preferA;
            }

            if (assignToA)
            {
                teamA.Add(checkIn);
                teamCountsA[group] = aCount + 1;
            }
            else
            {
                teamB.Add(checkIn);
                teamCountsB[group] = bCount + 1;
            }
        }

        return (teamA, teamB);
    }
}
