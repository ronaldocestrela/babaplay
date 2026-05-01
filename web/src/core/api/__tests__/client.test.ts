import { describe, it, expect, beforeEach } from 'vitest'
import { http, HttpResponse } from 'msw'
import { apiClient } from '../client'
import { useAuthStore } from '@/features/auth/store/authStore'
import { server } from '@/test/server'
import { mockAuthResponse } from '@/test/handlers'

const BASE_URL = 'http://localhost:5050'

describe('apiClient', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
  })

  describe('request interceptor', () => {
    it('não deve adicionar Authorization header quando não há token', async () => {
      let capturedHeader: string | null = null

      server.use(
        http.get(`${BASE_URL}/api/v1/ping`, ({ request }) => {
          capturedHeader = request.headers.get('Authorization')
          return HttpResponse.json({ ok: true })
        }),
      )

      await apiClient.get('/api/v1/ping')
      expect(capturedHeader).toBeNull()
    })

    it('deve injetar Bearer token quando há accessToken no store', async () => {
      useAuthStore.getState().setTokens(mockAuthResponse)
      let capturedHeader: string | null = null

      server.use(
        http.get(`${BASE_URL}/api/v1/ping`, ({ request }) => {
          capturedHeader = request.headers.get('Authorization')
          return HttpResponse.json({ ok: true })
        }),
      )

      await apiClient.get('/api/v1/ping')
      expect(capturedHeader).toBe(`Bearer ${mockAuthResponse.accessToken}`)
    })
  })

  describe('response interceptor — renovação de token', () => {
    it('deve renovar o access token ao receber 401 e retentar', async () => {
      useAuthStore.getState().setTokens({
        ...mockAuthResponse,
        accessToken: 'expired-access-token',
      })

      let requestCount = 0

      server.use(
        http.get(`${BASE_URL}/api/v1/protected`, ({ request }) => {
          requestCount++
          const auth = request.headers.get('Authorization')
          if (auth === 'Bearer expired-access-token') {
            return HttpResponse.json(
              { title: 'UNAUTHORIZED', status: 401 },
              { status: 401 },
            )
          }
          return HttpResponse.json({ data: 'secret' })
        }),
      )

      const result = await apiClient.get('/api/v1/protected')
      expect(result.data).toEqual({ data: 'secret' })
      expect(requestCount).toBe(2)
      expect(useAuthStore.getState().accessToken).toBe('new-access-token')
    })

    it('deve rejeitar e limpar sessão quando refresh token é inválido', async () => {
      useAuthStore.getState().setTokens({
        ...mockAuthResponse,
        accessToken: 'expired-access-token',
        refreshToken: 'invalid-token-xyz',
      })

      server.use(
        http.get(`${BASE_URL}/api/v1/protected`, () =>
          HttpResponse.json({ title: 'UNAUTHORIZED', status: 401 }, { status: 401 }),
        ),
      )

      await expect(apiClient.get('/api/v1/protected')).rejects.toBeDefined()
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
    })
  })
})
