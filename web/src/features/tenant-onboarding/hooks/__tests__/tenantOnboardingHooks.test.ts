import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createWrapper } from '@/test/utils'
import { useAssociationStatus, useCreateAssociation } from '../index'

describe('tenant onboarding hooks', () => {
  it('deve criar associação com useCreateAssociation', async () => {
    const { result } = renderHook(() => useCreateAssociation(), { wrapper: createWrapper() })

    act(() => {
      result.current.createAssociation({
        name: 'Associação Hook',
        slug: 'associacao-hook',
        adminEmail: 'admin@hook.com',
        adminPassword: 'Admin1234',
      })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(result.current.isError).toBe(false)
  })

  it('deve expor errorCode para slug duplicado', async () => {
    const { result } = renderHook(() => useCreateAssociation(), { wrapper: createWrapper() })

    act(() => {
      result.current.createAssociation({
        name: 'Associação Duplicada',
        slug: 'taken-slug',
        adminEmail: 'admin@duplicada.com',
        adminPassword: 'Admin1234',
      })
    })

    await waitFor(() => expect(result.current.isError).toBe(true))
    expect(result.current.errorCode).toBe('TENANT_SLUG_TAKEN')
  })

  it('deve carregar status da associação com useAssociationStatus', async () => {
    const { result } = renderHook(() => useAssociationStatus('tenant-123'), {
      wrapper: createWrapper(),
    })

    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data?.provisioningStatus).toBe('Ready')
  })

  it('não deve executar query quando tenantId não é informado', () => {
    const { result } = renderHook(() => useAssociationStatus(undefined), {
      wrapper: createWrapper(),
    })

    expect(result.current.fetchStatus).toBe('idle')
  })
})
