import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import { useTenantSettings, useUpdateTenantSettings } from '../useTenantSettings'

describe('tenant settings hooks', () => {
  it('deve carregar configurações do tenant com useTenantSettings', async () => {
    const { result } = renderHook(() => useTenantSettings(), { wrapper: createWrapper() })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.id).toBe('tenant-123')
    expect(result.current.data?.name).toBe('Mock Tenant')
    expect(result.current.data?.playersPerTeam).toBe(11)
  })

  it('deve atualizar configurações do tenant com useUpdateTenantSettings', async () => {
    const logo = new File(['fake-image'], 'logo.png', { type: 'image/png' })
    const { result } = renderHook(() => useUpdateTenantSettings(), { wrapper: createWrapper() })

    act(() => {
      result.current.updateSettings({
        name: 'Tenant Hook',
        playersPerTeam: 8,
        logo,
        street: 'Rua Hook',
        number: '77',
        neighborhood: 'Centro',
        city: 'Santos',
        state: 'SP',
        zipCode: '11000-000',
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
    expect(result.current.errorCode).toBeNull()
  })
})
