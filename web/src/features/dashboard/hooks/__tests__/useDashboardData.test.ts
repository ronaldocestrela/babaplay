import { renderHook, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createWrapper } from '@/test/utils'
import { useDashboardData } from '../useDashboardData'
import { dashboardService } from '../../services/dashboardService'

vi.mock('../../services/dashboardService', () => ({
  dashboardService: {
    getPlayers: vi.fn(),
    getTeams: vi.fn(),
    getGameDays: vi.fn(),
    getMatches: vi.fn(),
    getCheckinsByGameDay: vi.fn(),
    getRanking: vi.fn(),
    getTopScorers: vi.fn(),
    getAttendanceRanking: vi.fn(),
    getCashFlow: vi.fn(),
    getDelinquency: vi.fn(),
    getMonthlySummary: vi.fn(),
  },
}))

function mockOperationalDefaults() {
  vi.mocked(dashboardService.getPlayers).mockResolvedValue([
    { id: 'player-1', isActive: true },
    { id: 'player-2', isActive: false },
  ])
  vi.mocked(dashboardService.getTeams).mockResolvedValue([
    { id: 'team-1', isActive: true },
  ])
  vi.mocked(dashboardService.getGameDays).mockResolvedValue([
    { id: 'gameday-1', scheduledAt: new Date().toISOString(), status: 'Confirmed' },
  ])
  vi.mocked(dashboardService.getMatches).mockResolvedValue([
    { id: 'match-1', status: 'InProgress' },
  ])
  vi.mocked(dashboardService.getCheckinsByGameDay).mockResolvedValue([
    { id: 'checkin-1', isActive: true },
  ])
}

describe('useDashboardData', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('deve consolidar os dados do dashboard quando todas as leituras funcionam', async () => {
    mockOperationalDefaults()

    vi.mocked(dashboardService.getRanking).mockResolvedValue([
      { rank: 1, playerId: 'player-1', scoreTotal: 10, attendanceCount: 2, goals: 1 },
    ])
    vi.mocked(dashboardService.getTopScorers).mockResolvedValue([
      { rank: 1, playerId: 'player-1', goals: 1, scoreTotal: 10 },
    ])
    vi.mocked(dashboardService.getAttendanceRanking).mockResolvedValue([
      { rank: 1, playerId: 'player-1', attendanceCount: 2, scoreTotal: 10 },
    ])

    vi.mocked(dashboardService.getCashFlow).mockResolvedValue({
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
      totalIncome: 1000,
      totalExpense: 400,
      balance: 600,
    })
    vi.mocked(dashboardService.getDelinquency).mockResolvedValue({
      referenceUtc: '2026-05-04T10:00:00.000Z',
      totalOpenAmount: 250,
    })
    vi.mocked(dashboardService.getMonthlySummary).mockResolvedValue({
      year: 2026,
      month: 5,
      monthlyFeesAmount: 500,
      monthlyFeesPaidAmount: 450,
      monthlyFeesOpenAmount: 50,
      cashIncome: 1000,
      cashExpense: 400,
      cashBalance: 600,
    })

    const { result } = renderHook(() => useDashboardData(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(result.current.data?.operational.activePlayers).toBe(1)
    expect(result.current.data?.operational.activeTeams).toBe(1)
    expect(result.current.data?.operational.todayCheckins).toBe(1)

    expect(result.current.data?.ranking.available).toBe(true)
    expect(result.current.data?.ranking.bestScore).toBe(10)

    expect(result.current.data?.financial.available).toBe(true)
    expect(result.current.data?.financial.cashBalance).toBe(600)
  })

  it('deve degradar apenas o bloco de ranking quando a API retorna 403', async () => {
    mockOperationalDefaults()

    const forbidden = {
      response: {
        status: 403,
        data: { title: 'FORBIDDEN' },
      },
    }

    vi.mocked(dashboardService.getRanking).mockRejectedValue(forbidden)
    vi.mocked(dashboardService.getTopScorers).mockRejectedValue(forbidden)
    vi.mocked(dashboardService.getAttendanceRanking).mockRejectedValue(forbidden)

    vi.mocked(dashboardService.getCashFlow).mockResolvedValue({
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
      totalIncome: 1000,
      totalExpense: 400,
      balance: 600,
    })
    vi.mocked(dashboardService.getDelinquency).mockResolvedValue({
      referenceUtc: '2026-05-04T10:00:00.000Z',
      totalOpenAmount: 250,
    })
    vi.mocked(dashboardService.getMonthlySummary).mockResolvedValue({
      year: 2026,
      month: 5,
      monthlyFeesAmount: 500,
      monthlyFeesPaidAmount: 450,
      monthlyFeesOpenAmount: 50,
      cashIncome: 1000,
      cashExpense: 400,
      cashBalance: 600,
    })

    const { result } = renderHook(() => useDashboardData(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(result.current.data?.ranking.available).toBe(false)
    expect(result.current.data?.ranking.errorCode).toBe('FORBIDDEN')
    expect(result.current.data?.financial.available).toBe(true)
  })

  it('deve usar período customizado quando informado', async () => {
    mockOperationalDefaults()

    vi.mocked(dashboardService.getRanking).mockResolvedValue([])
    vi.mocked(dashboardService.getTopScorers).mockResolvedValue([])
    vi.mocked(dashboardService.getAttendanceRanking).mockResolvedValue([])
    vi.mocked(dashboardService.getCashFlow).mockResolvedValue({
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
      totalIncome: 0,
      totalExpense: 0,
      balance: 0,
    })
    vi.mocked(dashboardService.getDelinquency).mockResolvedValue({
      referenceUtc: '2026-05-04T10:00:00.000Z',
      totalOpenAmount: 0,
    })
    vi.mocked(dashboardService.getMonthlySummary).mockResolvedValue({
      year: 2026,
      month: 5,
      monthlyFeesAmount: 0,
      monthlyFeesPaidAmount: 0,
      monthlyFeesOpenAmount: 0,
      cashIncome: 0,
      cashExpense: 0,
      cashBalance: 0,
    })

    const customFrom = '2026-05-01T00:00:00.000Z'
    const customTo = '2026-05-31T23:59:59.999Z'

    const { result } = renderHook(
      () => useDashboardData({ fromUtc: customFrom, toUtc: customTo }),
      { wrapper: createWrapper() },
    )

    await waitFor(() => expect(result.current.isSuccess).toBe(true))

    expect(dashboardService.getRanking).toHaveBeenCalledWith(
      expect.objectContaining({ fromUtc: customFrom, toUtc: customTo }),
    )
    expect(dashboardService.getCashFlow).toHaveBeenCalledWith(customFrom, customTo)
  })
})
