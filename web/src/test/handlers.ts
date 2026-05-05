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

const mockPlayers = [
  {
    id: 'player-1',
    userId: 'user-1',
    name: 'Joao Silva',
    nickname: 'JS10',
    phone: '11999990001',
    dateOfBirth: '1995-10-01',
    isActive: true,
    createdAt: '2026-01-10T10:00:00.000Z',
  },
  {
    id: 'player-2',
    userId: 'user-2',
    name: 'Carlos Lima',
    nickname: null,
    phone: null,
    dateOfBirth: null,
    isActive: true,
    createdAt: '2026-01-11T10:00:00.000Z',
  },
  {
    id: 'player-3',
    userId: 'user-3',
    name: 'Pedro Gomes',
    nickname: null,
    phone: null,
    dateOfBirth: null,
    isActive: false,
    createdAt: '2026-01-12T10:00:00.000Z',
  },
]

const mockPositions = [
  {
    id: 'position-1',
    tenantId: 'tenant-123',
    code: 'GK',
    name: 'Goleiro',
    description: 'Defende o gol',
    isActive: true,
    createdAt: '2026-01-01T00:00:00.000Z',
  },
  {
    id: 'position-2',
    tenantId: 'tenant-123',
    code: 'CB',
    name: 'Zagueiro',
    description: 'Defende a area',
    isActive: true,
    createdAt: '2026-01-01T00:00:00.000Z',
  },
  {
    id: 'position-3',
    tenantId: 'tenant-123',
    code: 'FW',
    name: 'Atacante',
    description: 'Finaliza jogadas',
    isActive: true,
    createdAt: '2026-01-01T00:00:00.000Z',
  },
]

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
  http.get(`${BASE_URL}/api/v1/player`, () => HttpResponse.json(mockPlayers)),

  // GET /api/v1/player/:id
  http.get(`${BASE_URL}/api/v1/player/:id`, ({ params }) => {
    const player = mockPlayers.find((item) => item.id === params.id)

    if (!player) {
      return HttpResponse.json(
        { title: 'PLAYER_NOT_FOUND', detail: 'Player not found', status: 404 },
        { status: 404 },
      )
    }

    return HttpResponse.json(player)
  }),

  // POST /api/v1/player
  http.post(`${BASE_URL}/api/v1/player`, async ({ request }) => {
    const body = (await request.json()) as {
      userId: string
      name: string
      nickname?: string | null
      phone?: string | null
      dateOfBirth?: string | null
    }

    if (!body.name || body.name.trim().length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_NAME', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    if (body.name === 'duplicate-player') {
      return HttpResponse.json(
        {
          title: 'PLAYER_ALREADY_EXISTS',
          detail: 'Player already exists for this user',
          status: 409,
        },
        { status: 409 },
      )
    }

    return HttpResponse.json(
      {
        id: 'player-new',
        userId: body.userId,
        name: body.name,
        nickname: body.nickname ?? null,
        phone: body.phone ?? null,
        dateOfBirth: body.dateOfBirth ?? null,
        isActive: true,
        createdAt: '2026-05-04T13:00:00.000Z',
      },
      { status: 201 },
    )
  }),

  // PUT /api/v1/player/:id
  http.put(`${BASE_URL}/api/v1/player/:id`, async ({ params, request }) => {
    if (params.id === 'player-missing') {
      return HttpResponse.json(
        { title: 'PLAYER_NOT_FOUND', detail: 'Player not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as {
      name: string
      nickname?: string | null
      phone?: string | null
      dateOfBirth?: string | null
    }

    return HttpResponse.json({
      id: params.id,
      userId: 'user-1',
      name: body.name,
      nickname: body.nickname ?? null,
      phone: body.phone ?? null,
      dateOfBirth: body.dateOfBirth ?? null,
      isActive: true,
      createdAt: '2026-01-10T10:00:00.000Z',
    })
  }),

  // DELETE /api/v1/player/:id
  http.delete(`${BASE_URL}/api/v1/player/:id`, ({ params }) => {
    if (params.id === 'player-missing') {
      return HttpResponse.json(
        { title: 'PLAYER_NOT_FOUND', detail: 'Player not found', status: 404 },
        { status: 404 },
      )
    }

    return new HttpResponse(null, { status: 204 })
  }),

  // PUT /api/v1/player/:id/positions
  http.put(`${BASE_URL}/api/v1/player/:id/positions`, async ({ params, request }) => {
    const body = (await request.json()) as { positionIds: string[] }

    if (!Array.isArray(body.positionIds) || body.positionIds.length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_POSITION_ID', detail: 'Position ids are required', status: 422 },
        { status: 422 },
      )
    }

    if (body.positionIds.length > 3) {
      return HttpResponse.json(
        {
          title: 'POSITIONS_LIMIT_EXCEEDED',
          detail: 'A player can have at most 3 positions',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (new Set(body.positionIds).size !== body.positionIds.length) {
      return HttpResponse.json(
        {
          title: 'DUPLICATE_POSITIONS',
          detail: 'Duplicated position ids are not allowed',
          status: 422,
        },
        { status: 422 },
      )
    }

    const hasUnknownPosition = body.positionIds.some((positionId) =>
      positionId.startsWith('position-missing'),
    )

    if (hasUnknownPosition) {
      return HttpResponse.json(
        {
          title: 'POSITION_NOT_FOUND',
          detail: 'Position was not found',
          status: 404,
        },
        { status: 404 },
      )
    }

    return HttpResponse.json({
      playerId: params.id,
      positionIds: body.positionIds,
      updatedAt: '2026-05-04T13:00:00.000Z',
    })
  }),

  // GET /api/v1/position
  http.get(`${BASE_URL}/api/v1/position`, () => HttpResponse.json(mockPositions)),

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
