import { http, HttpResponse } from 'msw'
import type { AuthResponse, UserProfile } from '@/features/auth/types'

const BASE_URL = 'http://localhost:5050'

export const mockAuthResponse: AuthResponse = {
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
  expiresIn: 3600,
  tokenType: 'Bearer',
}

export const mockUserProfile: UserProfile = {
  id: 'user-123',
  email: 'test@example.com',
  roles: ['Player'],
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
}

export const handlers = [
  // POST /api/v1/auth/login
  http.post(`${BASE_URL}/api/v1/auth/login`, async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string }

    if (body.email === 'inactive@example.com') {
      return HttpResponse.json(
        { title: 'USER_INACTIVE', detail: 'User is inactive', status: 422 },
        { status: 422 },
      )
    }

    if (body.email !== 'test@example.com' || body.password !== 'password123') {
      return HttpResponse.json(
        { title: 'INVALID_CREDENTIALS', detail: 'Invalid credentials', status: 401 },
        { status: 401 },
      )
    }

    return HttpResponse.json(mockAuthResponse)
  }),

  // POST /api/v1/auth/refresh-token
  http.post(`${BASE_URL}/api/v1/auth/refresh-token`, async ({ request }) => {
    const body = (await request.json()) as { refreshToken: string }

    if (body.refreshToken === 'expired-token') {
      return HttpResponse.json(
        { title: 'TOKEN_EXPIRED', detail: 'Token has expired', status: 401 },
        { status: 401 },
      )
    }

    if (body.refreshToken !== 'mock-refresh-token') {
      return HttpResponse.json(
        { title: 'INVALID_TOKEN', detail: 'Invalid token', status: 401 },
        { status: 401 },
      )
    }

    return HttpResponse.json({
      ...mockAuthResponse,
      accessToken: 'new-access-token',
      refreshToken: 'new-refresh-token',
    })
  }),

  // POST /api/v1/auth/logout
  http.post(`${BASE_URL}/api/v1/auth/logout`, async ({ request }) => {
    const body = (await request.json()) as { refreshToken: string }

    if (!body.refreshToken || body.refreshToken === 'invalid-logout-token') {
      return HttpResponse.json(
        { title: 'INVALID_TOKEN', detail: 'Invalid token', status: 401 },
        { status: 401 },
      )
    }

    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/auth/me
  http.get(`${BASE_URL}/api/v1/auth/me`, ({ request }) => {
    const auth = request.headers.get('Authorization')

    if (!auth || !auth.startsWith('Bearer ')) {
      return HttpResponse.json(
        { title: 'UNAUTHORIZED', detail: 'Token required', status: 401 },
        { status: 401 },
      )
    }

    const token = auth.replace('Bearer ', '')
    if (token !== 'mock-access-token' && token !== 'new-access-token') {
      return HttpResponse.json(
        { title: 'UNAUTHORIZED', detail: 'Invalid token', status: 401 },
        { status: 401 },
      )
    }

    return HttpResponse.json(mockUserProfile)
  }),
]
