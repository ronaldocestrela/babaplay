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
    window.history.replaceState({}, '', 'http://localhost:3000/')
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

    it('deve injetar X-Tenant-Slug a partir da query string em localhost', async () => {
      window.history.replaceState({}, '', 'http://localhost:3000/?tenant=falcons')
      let capturedTenantHeader: string | null = null

      server.use(
        http.get(`${BASE_URL}/api/v1/ping`, ({ request }) => {
          capturedTenantHeader = request.headers.get('X-Tenant-Slug')
          return HttpResponse.json({ ok: true })
        }),
      )

      await apiClient.get('/api/v1/ping')
      expect(capturedTenantHeader).toBe('falcons')
    })

    it('deve injetar X-Tenant-Slug a partir do tenant persistido no store quando URL não possui tenant', async () => {
      useAuthStore.getState().setCurrentTenant({ slug: 'lions', source: 'query' })
      let capturedTenantHeader: string | null = null

      server.use(
        http.get(`${BASE_URL}/api/v1/ping`, ({ request }) => {
          capturedTenantHeader = request.headers.get('X-Tenant-Slug')
          return HttpResponse.json({ ok: true })
        }),
      )

      await apiClient.get('/api/v1/ping')
      expect(capturedTenantHeader).toBe('lions')
    })
  })

  describe('response interceptor — renovação de token', () => {
    it('não deve tentar refresh ou redirecionar quando login retorna 401', async () => {
      useAuthStore.getState().setTokens(mockAuthResponse)

      let refreshCount = 0

      server.use(
        http.post(`${BASE_URL}/api/v1/auth/login`, () =>
          HttpResponse.json(
            { title: 'INVALID_CREDENTIALS', status: 401 },
            { status: 401 },
          ),
        ),
        http.post(`${BASE_URL}/api/v1/auth/refresh-token`, () => {
          refreshCount++
          return HttpResponse.json(
            { title: 'INVALID_TOKEN', status: 401 },
            { status: 401 },
          )
        }),
      )

      await expect(
        apiClient.post('/api/v1/auth/login', {
          email: 'wrong@example.com',
          password: 'wrongpass',
        }),
      ).rejects.toMatchObject({ response: { status: 401 } })

      expect(refreshCount).toBe(0)
      expect(useAuthStore.getState().isAuthenticated).toBe(true)
    })

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

    it('deve enviar X-Tenant-Slug também na requisição de refresh token', async () => {
      window.history.replaceState({}, '', 'http://localhost:3000/?tenant=wolves')
      useAuthStore.getState().setTokens({
        ...mockAuthResponse,
        accessToken: 'expired-access-token',
      })

      let requestCount = 0
      let refreshTenantHeader: string | null = null

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
        http.post(`${BASE_URL}/api/v1/auth/refresh-token`, ({ request }) => {
          refreshTenantHeader = request.headers.get('X-Tenant-Slug')
          return HttpResponse.json({
            ...mockAuthResponse,
            accessToken: 'new-access-token',
            refreshToken: 'new-refresh-token',
          })
        }),
      )

      const result = await apiClient.get('/api/v1/protected')

      expect(result.data).toEqual({ data: 'secret' })
      expect(requestCount).toBe(2)
      expect(refreshTenantHeader).toBe('wolves')
    })

    it('deve enviar X-Tenant-Slug no refresh token usando tenant persistido no store quando URL não possui tenant', async () => {
      useAuthStore.getState().setCurrentTenant({ slug: 'hawks', source: 'query' })
      useAuthStore.getState().setTokens({
        ...mockAuthResponse,
        accessToken: 'expired-access-token',
      })

      let requestCount = 0
      let refreshTenantHeader: string | null = null

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
        http.post(`${BASE_URL}/api/v1/auth/refresh-token`, ({ request }) => {
          refreshTenantHeader = request.headers.get('X-Tenant-Slug')
          return HttpResponse.json({
            ...mockAuthResponse,
            accessToken: 'new-access-token',
            refreshToken: 'new-refresh-token',
          })
        }),
      )

      const result = await apiClient.get('/api/v1/protected')

      expect(result.data).toEqual({ data: 'secret' })
      expect(requestCount).toBe(2)
      expect(refreshTenantHeader).toBe('hawks')
    })
  })
})
