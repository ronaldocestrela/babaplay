import { renderHook, waitFor, act } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { http, HttpResponse } from 'msw'
import { useLogin } from '../useLogin'
import { useAuthStore } from '../../store/authStore'
import { createWrapper } from '@/test/utils'
import { server } from '@/test/server'
import { mockAuthResponse, mockUserProfile } from '@/test/handlers'

const mockNavigate = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

describe('useLogin', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
    mockNavigate.mockClear()
  })

  it('deve autenticar e navegar para / em login válido', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    act(() => {
      result.current.login({ email: 'test@example.com', password: 'password123' })
    })

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(true)
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/' })
    })

    expect(useAuthStore.getState().currentTenant?.slug).toBe('mock-tenant')
    expect(useAuthStore.getState().currentTenant?.source).toBe('profile')
  })

  it('deve retornar isError=true com credenciais inválidas', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    act(() => {
      result.current.login({ email: 'wrong@example.com', password: 'wrongpass' })
    })

    await waitFor(() => {
      expect(result.current.isError).toBe(true)
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
    })

    expect(mockNavigate).not.toHaveBeenCalled()
  })

  it('deve expor errorCode USER_INACTIVE para usuário inativo', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    act(() => {
      result.current.login({ email: 'inactive@example.com', password: 'password123' })
    })

    await waitFor(() => {
      expect(result.current.isError).toBe(true)
      expect(result.current.errorCode).toBe('USER_INACTIVE')
    })
  })

  it('deve expor errorCode INVALID_CREDENTIALS para credenciais inválidas', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    act(() => {
      result.current.login({ email: 'wrong@example.com', password: 'wrongpass' })
    })

    await waitFor(() => {
      expect(result.current.errorCode).toBe('INVALID_CREDENTIALS')
    })
  })

  it('deve iniciar com isPending=false e voltar a false após login bem sucedido', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    expect(result.current.isPending).toBe(false)

    act(() => {
      result.current.login({ email: 'test@example.com', password: 'password123' })
    })

    await waitFor(() => expect(result.current.isPending).toBe(false))
    expect(useAuthStore.getState().isAuthenticated).toBe(true)
  })

  it('deve navegar para seleção manual quando usuário possui múltiplos tenants', async () => {
    server.use(
      http.post('http://localhost:5050/api/v1/auth/login', () => HttpResponse.json(mockAuthResponse)),
      http.get('http://localhost:5050/api/v1/auth/me', () =>
        HttpResponse.json({
          ...mockUserProfile,
          primaryTenant: null,
          tenants: [
            {
              id: 'tenant-1',
              name: 'Clube A',
              slug: 'clube-a',
              isOwner: true,
              joinedAt: '2024-01-01T00:00:00Z',
            },
            {
              id: 'tenant-2',
              name: 'Clube B',
              slug: 'clube-b',
              isOwner: false,
              joinedAt: '2024-02-01T00:00:00Z',
            },
          ],
        }),
      ),
    )

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() })

    act(() => {
      result.current.login({ email: 'test@example.com', password: 'password123' })
    })

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(true)
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/select-tenant' })
    })

    expect(useAuthStore.getState().currentTenant).toBeNull()
  })
})
