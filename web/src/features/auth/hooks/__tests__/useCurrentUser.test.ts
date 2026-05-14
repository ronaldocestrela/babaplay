import { renderHook, waitFor } from '@testing-library/react'
import { describe, it, expect, beforeEach } from 'vitest'
import { http, HttpResponse } from 'msw'
import { useCurrentUser } from '../useCurrentUser'
import { useAuthStore } from '../../store/authStore'
import { createWrapper } from '@/test/utils'
import { mockAuthResponse, mockUserProfile } from '@/test/handlers'
import { server } from '@/test/server'

describe('useCurrentUser', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
  })

  it('deve carregar e retornar UserProfile com token válido', async () => {
    useAuthStore.getState().setTokens(mockAuthResponse)

    const { result } = renderHook(() => useCurrentUser(), { wrapper: createWrapper() })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(result.current.data?.id).toBe(mockUserProfile.id)
    expect(result.current.data?.email).toBe(mockUserProfile.email)
    expect(result.current.data?.roles).toContain('Player')
  })

  it('deve armazenar UserProfile no store após fetch bem sucedido', async () => {
    useAuthStore.getState().setTokens(mockAuthResponse)

    const { result } = renderHook(() => useCurrentUser(), { wrapper: createWrapper() })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(useAuthStore.getState().currentUser?.email).toBe(mockUserProfile.email)
    expect(useAuthStore.getState().currentTenant?.slug).toBe('mock-tenant')
  })

  it('deve iniciar com isLoading=false quando não há token', () => {
    // Sem token → query desabilitada
    const { result } = renderHook(() => useCurrentUser(), { wrapper: createWrapper() })
    expect(result.current.isLoading).toBe(false)
    expect(result.current.data).toBeUndefined()
  })

  it('deve retornar isError=true quando o servidor retorna erro não-401', async () => {
    // Sobrescreve handler para retornar 500 — não aciona interceptor de refresh
    server.use(
      http.get('http://localhost:5050/api/v1/auth/me', () =>
        HttpResponse.json({ title: 'INTERNAL_ERROR', status: 500 }, { status: 500 }),
      ),
    )
    useAuthStore.getState().setTokens(mockAuthResponse)

    const { result } = renderHook(() => useCurrentUser(), { wrapper: createWrapper() })

    await waitFor(() => {
      expect(result.current.isError).toBe(true)
    })
  })
})
