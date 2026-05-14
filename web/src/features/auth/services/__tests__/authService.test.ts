import { describe, it, expect, beforeEach } from 'vitest'
import { authService } from '../authService'
import { mockAuthResponse, mockUserProfile } from '@/test/handlers'
import { useAuthStore } from '../../store/authStore'

describe('authService', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
  })

  describe('login', () => {
    it('deve retornar AuthResponse com credenciais válidas', async () => {
      const result = await authService.login({
        email: 'test@example.com',
        password: 'password123',
      })
      expect(result.accessToken).toBe(mockAuthResponse.accessToken)
      expect(result.refreshToken).toBe(mockAuthResponse.refreshToken)
      expect(result.expiresIn).toBe(mockAuthResponse.expiresIn)
    })

    it('deve lançar erro 401 com credenciais inválidas', async () => {
      await expect(
        authService.login({ email: 'wrong@example.com', password: 'wrongpass' }),
      ).rejects.toMatchObject({ response: { status: 401 } })
    })

    it('deve lançar erro 422 com usuário inativo', async () => {
      await expect(
        authService.login({ email: 'inactive@example.com', password: 'password123' }),
      ).rejects.toMatchObject({ response: { status: 422 } })
    })
  })

  describe('refreshToken', () => {
    it('deve renovar tokens com refresh token válido', async () => {
      const result = await authService.refreshToken('mock-refresh-token')
      expect(result.accessToken).toBe('new-access-token')
      expect(result.refreshToken).toBe('new-refresh-token')
    })

    it('deve lançar erro 401 com refresh token expirado', async () => {
      await expect(authService.refreshToken('expired-token')).rejects.toMatchObject({
        response: { status: 401 },
      })
    })

    it('deve lançar erro 401 com refresh token inválido', async () => {
      await expect(authService.refreshToken('invalid-token-xyz')).rejects.toMatchObject({
        response: { status: 401 },
      })
    })
  })

  describe('logout', () => {
    it('deve realizar logout com refresh token válido sem erros', async () => {
      await expect(authService.logout('mock-refresh-token')).resolves.toBeUndefined()
    })

    it('deve lançar erro 401 com refresh token inválido no logout', async () => {
      await expect(authService.logout('invalid-logout-token')).rejects.toMatchObject({
        response: { status: 401 },
      })
    })
  })

  describe('getCurrentUser', () => {
    it('deve retornar UserProfile com token válido', async () => {
      useAuthStore.getState().setTokens(mockAuthResponse)
      const result = await authService.getCurrentUser()
      expect(result.id).toBe(mockUserProfile.id)
      expect(result.email).toBe(mockUserProfile.email)
      expect(result.roles).toEqual(mockUserProfile.roles)
      expect(result.isActive).toBe(true)
    })

    it('deve lançar erro 401 sem token de autorização', async () => {
      // store limpo (clearTokens no beforeEach) → sem Bearer header
      await expect(authService.getCurrentUser()).rejects.toMatchObject({
        response: { status: 401 },
      })
    })
  })
})
