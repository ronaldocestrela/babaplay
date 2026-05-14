import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import {
  useChangeTenantGameDayOptionStatus,
  useCreateTenantGameDayOption,
  useTenantGameDayOptions,
} from '../useTenantGameDayOptions'

describe('tenant game day option hooks', () => {
  it('deve carregar opções de dia de jogo do tenant', async () => {
    const { result } = renderHook(() => useTenantGameDayOptions(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.length).toBeGreaterThan(0)
  })

  it('deve criar opção de dia de jogo do tenant', async () => {
    const { result } = renderHook(() => useCreateTenantGameDayOption(), { wrapper: createWrapper() })

    act(() => {
      result.current.createOption({
        dayOfWeek: 5,
        localStartTime: '21:00:00',
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
    expect(result.current.errorCode).toBeNull()
  })

  it('deve expor erro de duplicidade ao criar opção', async () => {
    const { result } = renderHook(() => useCreateTenantGameDayOption(), { wrapper: createWrapper() })

    act(() => {
      result.current.createOption({
        dayOfWeek: 2,
        localStartTime: '20:00:00',
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('TENANT_GAMEDAY_OPTION_ALREADY_EXISTS')
  })

  it('deve alterar status da opção de dia de jogo do tenant', async () => {
    const create = renderHook(() => useCreateTenantGameDayOption(), { wrapper: createWrapper() })

    act(() => {
      create.result.current.createOption({
        dayOfWeek: 6,
        localStartTime: '10:00:00',
      })
    })

    await waitFor(() => expect(create.result.current.isPending).toBe(false))

    const options = renderHook(() => useTenantGameDayOptions(), { wrapper: createWrapper() })
    await waitFor(() => expect(options.result.current.isSuccess).toBe(true))

    const target = options.result.current.data?.find(
      (item) => item.dayOfWeek === 6 && item.localStartTime === '10:00:00',
    )
    expect(target).toBeDefined()

    const change = renderHook(() => useChangeTenantGameDayOptionStatus(), { wrapper: createWrapper() })

    act(() => {
      change.result.current.changeStatus({
        id: target!.id,
        isActive: false,
      })
    })

    await waitFor(() => expect(change.result.current.isPending).toBe(false))
    expect(change.result.current.isError).toBe(false)
    expect(change.result.current.errorCode).toBeNull()
  })
})
