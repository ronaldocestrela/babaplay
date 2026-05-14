import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import {
  useCancelCheckin,
  useCheckinGameDays,
  useCheckinPlayers,
  useCheckinsByGameDay,
  useCheckinsByPlayer,
  useCreateCheckin,
} from '../index'

describe('checkin hooks', () => {
  it('deve listar jogadores para seleção', async () => {
    const { result } = renderHook(() => useCheckinPlayers(), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve listar dias de jogo para seleção', async () => {
    const { result } = renderHook(() => useCheckinGameDays(), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve listar check-ins por game day', async () => {
    const { result } = renderHook(() => useCheckinsByGameDay('gameday-1'), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('não deve buscar check-ins por game day sem id', async () => {
    const { result } = renderHook(() => useCheckinsByGameDay(undefined), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toBeUndefined()
  })

  it('deve listar check-ins por jogador', async () => {
    const { result } = renderHook(() => useCheckinsByPlayer('player-1'), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('não deve buscar check-ins por jogador sem id', async () => {
    const { result } = renderHook(() => useCheckinsByPlayer(undefined), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toBeUndefined()
  })

  it('deve criar check-in', async () => {
    const { result } = renderHook(() => useCreateCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.createCheckin({
        playerId: 'player-2',
        gameDayId: 'gameday-1',
        checkedInAtUtc: '2026-05-05T10:00:00.000Z',
        latitude: -23.551,
        longitude: -46.633,
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor erro de check-in duplicado', async () => {
    const { result } = renderHook(() => useCreateCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.createCheckin({
        playerId: 'player-1',
        gameDayId: 'gameday-1',
        checkedInAtUtc: '2026-05-05T10:00:00.000Z',
        latitude: -23.551,
        longitude: -46.633,
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('CHECKIN_ALREADY_EXISTS')
  })

  it('deve expor erro de check-in fora do raio', async () => {
    const { result } = renderHook(() => useCreateCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.createCheckin({
        playerId: 'player-2',
        gameDayId: 'gameday-1',
        checkedInAtUtc: '2026-05-05T10:00:00.000Z',
        latitude: 89,
        longitude: 179,
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('CHECKIN_OUTSIDE_ALLOWED_RADIUS')
  })

  it('deve cancelar check-in', async () => {
    const { result } = renderHook(() => useCancelCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.cancelCheckin({
        id: 'checkin-1',
        gameDayId: 'gameday-1',
        playerId: 'player-1',
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve cancelar check-in sem gameDayId/playerId opcionais', async () => {
    const { result } = renderHook(() => useCancelCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.cancelCheckin({ id: 'checkin-1' })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor erro quando check-in não existe', async () => {
    const { result } = renderHook(() => useCancelCheckin(), { wrapper: createWrapper() })

    act(() => {
      result.current.cancelCheckin({ id: 'checkin-missing' })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('CHECKIN_NOT_FOUND')
  })
})
