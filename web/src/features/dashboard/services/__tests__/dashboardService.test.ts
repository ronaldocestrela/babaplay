import { describe, it, expect } from 'vitest'
import { dashboardService } from '../dashboardService'

describe('dashboardService', () => {
  it('deve buscar listas operacionais', async () => {
    const [players, teams, gameDays, matches] = await Promise.all([
      dashboardService.getPlayers(),
      dashboardService.getTeams(),
      dashboardService.getGameDays(),
      dashboardService.getMatches(),
    ])

    expect(players.length).toBeGreaterThan(0)
    expect(teams.length).toBeGreaterThan(0)
    expect(gameDays.length).toBeGreaterThan(0)
    expect(matches.length).toBeGreaterThan(0)
  })

  it('deve buscar ranking e derivados', async () => {
    const params = {
      page: 1,
      pageSize: 5,
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
    }

    const [ranking, topScorers, attendance] = await Promise.all([
      dashboardService.getRanking(params),
      dashboardService.getTopScorers(params),
      dashboardService.getAttendanceRanking(params),
    ])

    expect(ranking[0]?.scoreTotal).toBeTypeOf('number')
    expect(topScorers[0]?.goals).toBeTypeOf('number')
    expect(attendance[0]?.attendanceCount).toBeTypeOf('number')
  })

  it('deve buscar dados financeiros', async () => {
    const [cashFlow, delinquency, monthlySummary] = await Promise.all([
      dashboardService.getCashFlow('2026-05-01T00:00:00.000Z', '2026-05-31T23:59:59.999Z'),
      dashboardService.getDelinquency('2026-05-04T10:00:00.000Z'),
      dashboardService.getMonthlySummary(2026, 5),
    ])

    expect(cashFlow.balance).toBeTypeOf('number')
    expect(delinquency.totalOpenAmount).toBeTypeOf('number')
    expect(monthlySummary.monthlyFeesPaidAmount).toBeTypeOf('number')
  })

  it('deve buscar check-ins por dia de jogo', async () => {
    const checkins = await dashboardService.getCheckinsByGameDay('gameday-1')
    expect(checkins.length).toBeGreaterThan(0)
  })
})
