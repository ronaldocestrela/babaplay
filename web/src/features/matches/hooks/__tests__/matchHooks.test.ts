import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import {
  useChangeMatchStatus,
  useCreateMatch,
  useDeleteMatch,
  useMatch,
  useMatchGameDays,
  useMatches,
  useMatchTeams,
  useUpdateMatch,
} from '../index'

describe('match hooks', () => {
  it('deve listar partidas', async () => {
    const { result } = renderHook(() => useMatches(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve buscar partida por id', async () => {
    const { result } = renderHook(() => useMatch('match-1'), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.id).toBe('match-1')
  })

  it('deve listar game days e times para formulário', async () => {
    const { result: gameDays } = renderHook(() => useMatchGameDays(), { wrapper: createWrapper() })
    const { result: teams } = renderHook(() => useMatchTeams(), { wrapper: createWrapper() })

    await waitFor(() => expect(gameDays.current.isSuccess).toBe(true))
    await waitFor(() => expect(teams.current.isSuccess).toBe(true))

    expect(gameDays.current.data?.length).toBeGreaterThan(0)
    expect(teams.current.data?.length).toBeGreaterThan(0)
  })

  it('deve criar partida', async () => {
    const { result } = renderHook(() => useCreateMatch(), { wrapper: createWrapper() })

    act(() => {
      result.current.createMatch({
        gameDayId: 'gameday-2',
        homeTeamId: 'team-1',
        awayTeamId: 'team-3',
        description: 'Final',
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor erro ao criar partida com times iguais', async () => {
    const { result } = renderHook(() => useCreateMatch(), { wrapper: createWrapper() })

    act(() => {
      result.current.createMatch({
        gameDayId: 'gameday-2',
        homeTeamId: 'team-1',
        awayTeamId: 'team-1',
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('TEAMS_MUST_BE_DIFFERENT')
  })

  it('deve atualizar partida', async () => {
    const { result } = renderHook(() => useUpdateMatch(), { wrapper: createWrapper() })

    act(() => {
      result.current.updateMatch({
        id: 'match-1',
        payload: {
          gameDayId: 'gameday-1',
          homeTeamId: 'team-1',
          awayTeamId: 'team-2',
          description: 'Atualizada',
        },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve alterar status da partida', async () => {
    const { result } = renderHook(() => useChangeMatchStatus(), { wrapper: createWrapper() })

    act(() => {
      result.current.changeMatchStatus({
        id: 'match-1',
        payload: { status: 'Completed' },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve excluir partida', async () => {
    const { result } = renderHook(() => useDeleteMatch(), { wrapper: createWrapper() })

    act(() => {
      result.current.deleteMatch('match-2')
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })
})
