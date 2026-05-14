import { renderHook, waitFor, act } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useLogout } from '../useLogout'
import { useAuthStore } from '../../store/authStore'
import { createWrapper } from '@/test/utils'
import type { UserProfile } from '../../types'
import { mockAuthResponse } from '@/test/handlers'

const mockNavigate = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

const mockUser: UserProfile = {
  id: 'user-123',
  email: 'test@example.com',
  roles: ['Player'],
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
}

describe('useLogout', () => {
  beforeEach(() => {
    useAuthStore.getState().setTokens(mockAuthResponse)
    useAuthStore.getState().setCurrentUser(mockUser)
    mockNavigate.mockClear()
  })

  it('deve limpar store e navegar para /login em logout bem sucedido', async () => {
    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() })

    act(() => {
      result.current.logout()
    })

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
      expect(useAuthStore.getState().accessToken).toBeNull()
      expect(useAuthStore.getState().refreshToken).toBeNull()
      expect(useAuthStore.getState().currentUser).toBeNull()
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/login' })
    })
  })

  it('deve iniciar com isPending=false', () => {
    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() })
    expect(result.current.isPending).toBe(false)
  })

  it('deve limpar store mesmo quando o servidor retorna erro', async () => {
    // Simula token inválido — servidor retorna 401, mas frontend ainda limpa sessão
    useAuthStore.getState().setTokens({ ...mockAuthResponse, refreshToken: 'invalid-logout-token' })

    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() })

    act(() => {
      result.current.logout()
    })

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/login' })
    })
  })

  it('deve fazer logout mesmo sem refreshToken armazenado', async () => {
    useAuthStore.getState().setTokens({ ...mockAuthResponse, refreshToken: null })

    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() })

    act(() => {
      result.current.logout()
    })

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/login' })
    })
  })
})
