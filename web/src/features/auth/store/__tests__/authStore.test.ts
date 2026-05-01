import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '../authStore'

const mockAuth = {
  accessToken: 'access-token-abc',
  refreshToken: 'refresh-token-xyz',
  expiresIn: 3600,
  tokenType: 'Bearer',
}

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
  })

  it('deve iniciar sem autenticação', () => {
    const { isAuthenticated, accessToken, refreshToken } = useAuthStore.getState()
    expect(isAuthenticated).toBe(false)
    expect(accessToken).toBeNull()
    expect(refreshToken).toBeNull()
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
    useAuthStore.getState().clearTokens()
    const { isAuthenticated, accessToken, refreshToken } = useAuthStore.getState()
    expect(isAuthenticated).toBe(false)
    expect(accessToken).toBeNull()
    expect(refreshToken).toBeNull()
  })

  it('deve substituir tokens ao chamar setTokens novamente', () => {
    useAuthStore.getState().setTokens(mockAuth)
    useAuthStore.getState().setTokens({ ...mockAuth, accessToken: 'new-token' })
    expect(useAuthStore.getState().accessToken).toBe('new-token')
  })
})
