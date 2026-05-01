import { describe, it, expect } from 'vitest'
import { authService } from '../authService'
import { mockAuthResponse } from '@/test/handlers'

describe('authService', () => {
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
})
