import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import {
  useCreateTeam,
  useDeleteTeam,
  useTeam,
  useTeamPlayers,
  useTeams,
  useUpdateTeam,
  useUpdateTeamPlayers,
} from '../index'

describe('team hooks', () => {
  it('deve listar times', async () => {
    const { result } = renderHook(() => useTeams(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve buscar time por id', async () => {
    const { result } = renderHook(() => useTeam('team-1'), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.id).toBe('team-1')
  })

  it('não deve executar busca de time sem id', async () => {
    const { result } = renderHook(() => useTeam(undefined), { wrapper: createWrapper() })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })
    expect(result.current.data).toBeUndefined()
  })

  it('deve listar jogadores para elenco', async () => {
    const { result } = renderHook(() => useTeamPlayers(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve criar time', async () => {
    const { result } = renderHook(() => useCreateTeam(), { wrapper: createWrapper() })

    act(() => {
      result.current.createTeam({
        name: 'Time Hook',
        maxPlayers: 8,
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve atualizar time', async () => {
    const { result } = renderHook(() => useUpdateTeam(), { wrapper: createWrapper() })

    act(() => {
      result.current.updateTeam({
        id: 'team-1',
        payload: {
          name: 'Time Hook Editado',
          maxPlayers: 10,
        },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve atualizar elenco do time', async () => {
    const { result } = renderHook(() => useUpdateTeamPlayers(), { wrapper: createWrapper() })

    act(() => {
      result.current.updateTeamPlayers({
        id: 'team-1',
        payload: {
          playerIds: ['player-1', 'player-2'],
        },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor erro de elenco sem goleiro', async () => {
    const { result } = renderHook(() => useUpdateTeamPlayers(), { wrapper: createWrapper() })

    act(() => {
      result.current.updateTeamPlayers({
        id: 'team-1',
        payload: {
          playerIds: ['player-2'],
        },
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('TEAM_GOALKEEPER_REQUIRED')
  })

  it('deve excluir time', async () => {
    const { result } = renderHook(() => useDeleteTeam(), { wrapper: createWrapper() })

    act(() => {
      result.current.deleteTeam('team-2')
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })
})
