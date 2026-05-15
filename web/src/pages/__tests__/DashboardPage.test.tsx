import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { DashboardPage } from '../DashboardPage'

vi.mock('@/features/dashboard/hooks/useDashboardData', () => ({
  useDashboardData: vi.fn(),
}))

import { useDashboardData } from '@/features/dashboard/hooks/useDashboardData'

describe('DashboardPage', () => {
  it('deve exibir loading enquanto carrega', () => {
    vi.mocked(useDashboardData).mockReturnValue({
      isLoading: true,
      isError: false,
      data: undefined,
      error: null,
    } as unknown as ReturnType<typeof useDashboardData>)

    render(<DashboardPage />)

    expect(screen.getByText(/carregando dashboard/i)).toBeInTheDocument()
  })

  it('deve exibir cards e blocos principais quando sucesso', () => {
    vi.mocked(useDashboardData).mockReturnValue({
      isLoading: false,
      isError: false,
      error: null,
      data: {
        operational: {
          activePlayers: 12,
          activeTeams: 4,
          upcomingGameDays: 3,
          liveMatches: 1,
          todayCheckins: 9,
        },
        ranking: {
          available: true,
          errorCode: null,
          bestScore: 21,
          topScorers: [{ rank: 1, playerId: 'p-1', goals: 6, scoreTotal: 21 }],
          attendanceLeaders: [{ rank: 1, playerId: 'p-2', attendanceCount: 8, scoreTotal: 19 }],
        },
        financial: {
          available: true,
          errorCode: null,
          cashBalance: 1300,
          openAmount: 420,
          monthlyFeesPaidAmount: 900,
        },
      },
    } as unknown as ReturnType<typeof useDashboardData>)

    render(<DashboardPage />)

    expect(screen.getByText(/jogadores ativos/i)).toBeInTheDocument()
    expect(screen.getByText('12')).toBeInTheDocument()
    expect(screen.getByText(/melhor score/i)).toBeInTheDocument()
    expect(screen.getByText('21')).toBeInTheDocument()
    expect(screen.getByText(/top presença/i)).toBeInTheDocument()
    expect(screen.getByText(/8 presenças/i)).toBeInTheDocument()
    expect(screen.getByText(/saldo em caixa/i)).toBeInTheDocument()
    expect(screen.getByText('R$ 1.300,00')).toBeInTheDocument()
  })

  it('deve exibir indisponibilidade por bloco quando sem permissão', () => {
    vi.mocked(useDashboardData).mockReturnValue({
      isLoading: false,
      isError: false,
      error: null,
      data: {
        operational: {
          activePlayers: 12,
          activeTeams: 4,
          upcomingGameDays: 3,
          liveMatches: 1,
          todayCheckins: 9,
        },
        ranking: {
          available: false,
          errorCode: 'FORBIDDEN',
          bestScore: 0,
          topScorers: [],
          attendanceLeaders: [],
        },
        financial: {
          available: false,
          errorCode: 'FORBIDDEN',
          cashBalance: 0,
          openAmount: 0,
          monthlyFeesPaidAmount: 0,
        },
      },
    } as unknown as ReturnType<typeof useDashboardData>)

    render(<DashboardPage />)

    expect(screen.getByText(/ranking indisponível/i)).toBeInTheDocument()
    expect(screen.getByText(/financeiro indisponível/i)).toBeInTheDocument()
  })

  it('deve aplicar período personalizado e chamar hook com intervalo selecionado', async () => {
    vi.mocked(useDashboardData).mockReturnValue({
      isLoading: false,
      isError: false,
      error: null,
      data: {
        operational: {
          activePlayers: 12,
          activeTeams: 4,
          upcomingGameDays: 3,
          liveMatches: 1,
          todayCheckins: 9,
        },
        ranking: {
          available: true,
          errorCode: null,
          bestScore: 21,
          topScorers: [{ rank: 1, playerId: 'p-1', goals: 6, scoreTotal: 21 }],
          attendanceLeaders: [{ rank: 1, playerId: 'p-2', attendanceCount: 8, scoreTotal: 19 }],
        },
        financial: {
          available: true,
          errorCode: null,
          cashBalance: 1300,
          openAmount: 420,
          monthlyFeesPaidAmount: 900,
        },
      },
    } as unknown as ReturnType<typeof useDashboardData>)

    render(<DashboardPage />)

    await userEvent.click(screen.getByRole('button', { name: /personalizado/i }))
    await userEvent.type(screen.getByLabelText(/de/i), '2026-05-01')
    await userEvent.type(screen.getByLabelText(/até/i), '2026-05-31')
    await userEvent.click(screen.getByRole('button', { name: /aplicar período/i }))

    expect(useDashboardData).toHaveBeenCalledWith({
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
    })
  })
})
