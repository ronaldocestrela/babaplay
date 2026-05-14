import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import {
  useCreatePlayer,
  useDeletePlayer,
  usePlayer,
  usePlayers,
  usePositions,
  useUpdatePlayer,
  useUpdatePlayerPositions,
} from '../index'

describe('players hooks', () => {
  it('deve listar jogadores com usePlayers', async () => {
    const { result } = renderHook(() => usePlayers(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve buscar jogador por id com usePlayer', async () => {
    const { result } = renderHook(() => usePlayer('player-1'), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.id).toBe('player-1')
  })

  it('não deve executar busca de jogador sem id', async () => {
    const { result } = renderHook(() => usePlayer(undefined), { wrapper: createWrapper() })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })
    expect(result.current.data).toBeUndefined()
  })

  it('deve listar posições com usePositions', async () => {
    const { result } = renderHook(() => usePositions(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve criar jogador com useCreatePlayer', async () => {
    const { result } = renderHook(() => useCreatePlayer(), { wrapper: createWrapper() })

    act(() => {
      result.current.createPlayer({
        userId: 'user-777',
        name: 'Criado Hook',
        nickname: null,
        phone: null,
        dateOfBirth: null,
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor errorCode em conflito de criação', async () => {
    const { result } = renderHook(() => useCreatePlayer(), { wrapper: createWrapper() })

    act(() => {
      result.current.createPlayer({
        userId: 'user-conflict',
        name: 'duplicate-player',
        nickname: null,
        phone: null,
        dateOfBirth: null,
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('PLAYER_ALREADY_EXISTS')
  })

  it('deve atualizar jogador com useUpdatePlayer', async () => {
    const { result } = renderHook(() => useUpdatePlayer(), { wrapper: createWrapper() })

    act(() => {
      result.current.updatePlayer({
        id: 'player-1',
        payload: {
          name: 'Atualizado Hook',
          nickname: 'UH',
          phone: '11988887777',
          dateOfBirth: '1997-04-12',
        },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve excluir jogador com useDeletePlayer', async () => {
    const { result } = renderHook(() => useDeletePlayer(), { wrapper: createWrapper() })

    act(() => {
      result.current.deletePlayer('player-2')
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve atualizar posições do jogador com useUpdatePlayerPositions', async () => {
    const { result } = renderHook(() => useUpdatePlayerPositions(), { wrapper: createWrapper() })

    act(() => {
      result.current.updatePlayerPositions({
        id: 'player-1',
        payload: {
          positionIds: ['position-1', 'position-2', 'position-3'],
        },
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor erro de limite de posições', async () => {
    const { result } = renderHook(() => useUpdatePlayerPositions(), { wrapper: createWrapper() })

    act(() => {
      result.current.updatePlayerPositions({
        id: 'player-1',
        payload: {
          positionIds: ['position-1', 'position-2', 'position-3', 'position-4'],
        },
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('POSITIONS_LIMIT_EXCEEDED')
  })

  it('deve expor erro de posições duplicadas', async () => {
    const { result } = renderHook(() => useUpdatePlayerPositions(), { wrapper: createWrapper() })

    act(() => {
      result.current.updatePlayerPositions({
        id: 'player-1',
        payload: {
          positionIds: ['position-1', 'position-1'],
        },
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('DUPLICATE_POSITIONS')
  })

  it('deve expor erro quando posição não é encontrada', async () => {
    const { result } = renderHook(() => useUpdatePlayerPositions(), { wrapper: createWrapper() })

    act(() => {
      result.current.updatePlayerPositions({
        id: 'player-1',
        payload: {
          positionIds: ['position-missing-1'],
        },
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('POSITION_NOT_FOUND')
  })
})