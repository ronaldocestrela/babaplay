import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '../authStore'
import type { UserProfile } from '../../types'

const mockAuth = {
  accessToken: 'access-token-abc',
  refreshToken: 'refresh-token-xyz',
  expiresIn: 3600,
  tokenType: 'Bearer',
}

const mockUser: UserProfile = {
  id: 'user-123',
  email: 'test@example.com',
  roles: ['Player'],
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
}

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
  })

  it('deve iniciar sem autenticação', () => {
    const { isAuthenticated, accessToken, refreshToken, currentUser } = useAuthStore.getState()
    expect(isAuthenticated).toBe(false)
    expect(accessToken).toBeNull()
    expect(refreshToken).toBeNull()
    expect(currentUser).toBeNull()
  })

  it('deve autenticar o usuário ao chamar setTokens', () => {
    useAuthStore.getState().setTokens(mockAuth)
    const { isAuthenticated, accessToken, refreshToken } = useAuthStore.getState()
    expect(isAuthenticated).toBe(true)
    expect(accessToken).toBe(mockAuth.accessToken)
    expect(refreshToken).toBe(mockAuth.refreshToken)
  })

  it('deve limpar a sessão ao chamar clearTokens', () => {
    useAuthStore.getState().setTokens(mockAuth)
    useAuthStore.getState().setCurrentUser(mockUser)
    useAuthStore.getState().clearTokens()
    const { isAuthenticated, accessToken, refreshToken, currentUser } = useAuthStore.getState()
    expect(isAuthenticated).toBe(false)
    expect(accessToken).toBeNull()
    expect(refreshToken).toBeNull()
    expect(currentUser).toBeNull()
  })

  it('deve substituir tokens ao chamar setTokens novamente', () => {
    useAuthStore.getState().setTokens(mockAuth)
    useAuthStore.getState().setTokens({ ...mockAuth, accessToken: 'new-token' })
    expect(useAuthStore.getState().accessToken).toBe('new-token')
  })

  it('deve armazenar perfil do usuário ao chamar setCurrentUser', () => {
    useAuthStore.getState().setTokens(mockAuth)
    useAuthStore.getState().setCurrentUser(mockUser)
    const { currentUser } = useAuthStore.getState()
    expect(currentUser).toEqual(mockUser)
    expect(currentUser?.email).toBe('test@example.com')
    expect(currentUser?.roles).toContain('Player')
  })

  it('deve limpar currentUser ao chamar clearTokens', () => {
    useAuthStore.getState().setTokens(mockAuth)
    useAuthStore.getState().setCurrentUser(mockUser)
    useAuthStore.getState().clearTokens()
    expect(useAuthStore.getState().currentUser).toBeNull()
  })
})
