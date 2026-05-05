import { http, HttpResponse } from 'msw'
import type { AuthResponse, UserProfile } from '@/features/auth/types'
import type { TenantResponse } from '@/features/auth/types'

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

  // POST /api/v1/tenant
  http.post(`${BASE_URL}/api/v1/tenant`, async ({ request }) => {
    const body = (await request.json()) as { name: string; slug: string }

    if (!body.name) {
      return HttpResponse.json(
        { title: 'TENANT_NAME_REQUIRED', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.slug) {
      return HttpResponse.json(
        { title: 'TENANT_SLUG_REQUIRED', detail: 'Slug is required', status: 422 },
        { status: 422 },
      )
    }

    if (body.slug === 'taken-slug') {
      return HttpResponse.json(
        { title: 'TENANT_SLUG_TAKEN', detail: 'Slug already in use', status: 409 },
        { status: 409 },
      )
    }

    const response: TenantResponse = {
      id: 'tenant-123',
      name: body.name,
      slug: body.slug.toLowerCase(),
      provisioningStatus: 'Pending',
    }

    return HttpResponse.json(response, { status: 201 })
  }),

  // GET /api/v1/tenant/:id/status
  http.get(`${BASE_URL}/api/v1/tenant/:id/status`, ({ params }) => {
    const { id } = params

    if (id === 'unknown-tenant-id') {
      return HttpResponse.json(
        { title: 'TENANT_NOT_FOUND', detail: 'Tenant not found', status: 404 },
        { status: 404 },
      )
    }

    const response: TenantResponse = {
      id: id as string,
      name: 'Mock Tenant',
      slug: 'mock-tenant',
      provisioningStatus: 'Ready',
    }

    return HttpResponse.json(response)
  }),

  // GET /api/v1/player
  http.get(`${BASE_URL}/api/v1/player`, () =>
    HttpResponse.json([
      { id: 'player-1', isActive: true },
      { id: 'player-2', isActive: true },
      { id: 'player-3', isActive: false },
    ]),
  ),

  // GET /api/v1/team
  http.get(`${BASE_URL}/api/v1/team`, () =>
    HttpResponse.json([
      { id: 'team-1', isActive: true },
      { id: 'team-2', isActive: true },
    ]),
  ),

  // GET /api/v1/gameday
  http.get(`${BASE_URL}/api/v1/gameday`, () =>
    HttpResponse.json([
      { id: 'gameday-1', scheduledAt: '2026-05-04T13:00:00.000Z', status: 'Confirmed' },
      { id: 'gameday-2', scheduledAt: '2026-05-10T13:00:00.000Z', status: 'Pending' },
    ]),
  ),

  // GET /api/v1/match
  http.get(`${BASE_URL}/api/v1/match`, () =>
    HttpResponse.json([
      { id: 'match-1', status: 'InProgress' },
      { id: 'match-2', status: 'Scheduled' },
    ]),
  ),

  // GET /api/v1/checkin/gameday/:gameDayId
  http.get(`${BASE_URL}/api/v1/checkin/gameday/:gameDayId`, ({ params }) => {
    const { gameDayId } = params

    if (gameDayId === 'gameday-1') {
      return HttpResponse.json([
        { id: 'checkin-1', isActive: true },
        { id: 'checkin-2', isActive: true },
      ])
    }

    return HttpResponse.json([])
  }),

  // GET /api/v1/ranking
  http.get(`${BASE_URL}/api/v1/ranking`, () =>
    HttpResponse.json([
      { rank: 1, playerId: 'player-1', scoreTotal: 15, attendanceCount: 4, goals: 3 },
    ]),
  ),

  // GET /api/v1/ranking/top-scorers
  http.get(`${BASE_URL}/api/v1/ranking/top-scorers`, () =>
    HttpResponse.json([
      { rank: 1, playerId: 'player-1', goals: 3, scoreTotal: 15 },
    ]),
  ),

  // GET /api/v1/ranking/attendance
  http.get(`${BASE_URL}/api/v1/ranking/attendance`, () =>
    HttpResponse.json([
      { rank: 1, playerId: 'player-2', attendanceCount: 5, scoreTotal: 12 },
    ]),
  ),

  // GET /api/v1/financial/cash-flow
  http.get(`${BASE_URL}/api/v1/financial/cash-flow`, () =>
    HttpResponse.json({
      fromUtc: '2026-05-01T00:00:00.000Z',
      toUtc: '2026-05-31T23:59:59.999Z',
      totalIncome: 2000,
      totalExpense: 700,
      balance: 1300,
    }),
  ),

  // GET /api/v1/financial/delinquency
  http.get(`${BASE_URL}/api/v1/financial/delinquency`, () =>
    HttpResponse.json({
      referenceUtc: '2026-05-04T10:00:00.000Z',
      totalOpenAmount: 420,
    }),
  ),

  // GET /api/v1/financial/monthly-summary
  http.get(`${BASE_URL}/api/v1/financial/monthly-summary`, () =>
    HttpResponse.json({
      year: 2026,
      month: 5,
      monthlyFeesAmount: 1000,
      monthlyFeesPaidAmount: 900,
      monthlyFeesOpenAmount: 100,
      cashIncome: 2000,
      cashExpense: 700,
      cashBalance: 1300,
    }),
  ),
]
