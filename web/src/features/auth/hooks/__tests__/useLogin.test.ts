import { renderHook, waitFor, act } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useLogin } from '../useLogin'
import { useAuthStore } from '../../store/authStore'
import { createWrapper } from '@/test/utils'

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
})
