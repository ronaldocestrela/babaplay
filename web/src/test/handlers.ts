import { http, HttpResponse } from 'msw'
import type { AuthResponse, UserProfile } from '@/features/auth/types'
import type { TenantResponse } from '@/features/auth/types'

const BASE_URL = 'http://localhost:5050'

export const mockAuthResponse: AuthResponse = {
  accessToken: 'mock-access-token',
  refreshToken: 'mock-refresh-token',
  expiresIn: 3600,
  tokenType: 'Bearer',
  primaryTenant: {
    id: 'tenant-123',
    name: 'Mock Tenant',
    slug: 'mock-tenant',
    isOwner: true,
    joinedAt: '2024-01-01T00:00:00Z',
  },
  tenants: [
    {
      id: 'tenant-123',
      name: 'Mock Tenant',
      slug: 'mock-tenant',
      isOwner: true,
      joinedAt: '2024-01-01T00:00:00Z',
    },
  ],
}

export const mockUserProfile: UserProfile = {
  id: 'user-123',
  email: 'test@example.com',
  roles: ['Player'],
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  primaryTenant: {
    id: 'tenant-123',
    name: 'Mock Tenant',
    slug: 'mock-tenant',
    isOwner: true,
    joinedAt: '2024-01-01T00:00:00Z',
  },
  tenants: [
    {
      id: 'tenant-123',
      name: 'Mock Tenant',
      slug: 'mock-tenant',
      isOwner: true,
      joinedAt: '2024-01-01T00:00:00Z',
    },
  ],
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

type MockPosition = {
  id: string
  tenantId: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
}

let mockPositions: MockPosition[] = [
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

const mockCheckins = [
  {
    id: 'checkin-1',
    tenantId: 'tenant-123',
    playerId: 'player-1',
    gameDayId: 'gameday-1',
    checkedInAtUtc: '2026-05-04T13:10:00.000Z',
    latitude: -23.5505,
    longitude: -46.6333,
    distanceFromAssociationMeters: 42.5,
    isActive: true,
    createdAt: '2026-05-04T13:10:00.000Z',
    cancelledAtUtc: null,
  },
  {
    id: 'checkin-2',
    tenantId: 'tenant-123',
    playerId: 'player-2',
    gameDayId: 'gameday-1',
    checkedInAtUtc: '2026-05-04T13:12:00.000Z',
    latitude: -23.5512,
    longitude: -46.6328,
    distanceFromAssociationMeters: 50.2,
    isActive: true,
    createdAt: '2026-05-04T13:12:00.000Z',
    cancelledAtUtc: null,
  },
]

const mockTeams = [
  {
    id: 'team-1',
    tenantId: 'tenant-123',
    name: 'Time Azul',
    maxPlayers: 8,
    isActive: true,
    createdAt: '2026-05-01T10:00:00.000Z',
    playerIds: ['player-1'],
  },
  {
    id: 'team-2',
    tenantId: 'tenant-123',
    name: 'Time Laranja',
    maxPlayers: 10,
    isActive: true,
    createdAt: '2026-05-02T10:00:00.000Z',
    playerIds: [],
  },
  {
    id: 'team-3',
    tenantId: 'tenant-123',
    name: 'Time Verde',
    maxPlayers: 9,
    isActive: true,
    createdAt: '2026-05-03T10:00:00.000Z',
    playerIds: [],
  },
]

const mockGameDays = [
  {
    id: 'gameday-1',
    scheduledAt: '2026-05-10T13:00:00.000Z',
    status: 'Confirmed',
  },
  {
    id: 'gameday-2',
    scheduledAt: '2026-05-12T13:00:00.000Z',
    status: 'Pending',
  },
  {
    id: 'gameday-past',
    scheduledAt: '2025-05-01T13:00:00.000Z',
    status: 'Completed',
  },
]

const mockMatches = [
  {
    id: 'match-1',
    tenantId: 'tenant-123',
    gameDayId: 'gameday-1',
    homeTeamId: 'team-1',
    awayTeamId: 'team-2',
    description: 'Semifinal',
    status: 'InProgress',
    isActive: true,
    createdAt: '2026-05-04T10:00:00.000Z',
  },
  {
    id: 'match-2',
    tenantId: 'tenant-123',
    gameDayId: 'gameday-2',
    homeTeamId: 'team-2',
    awayTeamId: 'team-1',
    description: null,
    status: 'Scheduled',
    isActive: true,
    createdAt: '2026-05-04T10:05:00.000Z',
  },
]

let mockTenantGameDayOptions: Array<{
  id: string
  tenantId: string
  dayOfWeek: number
  localStartTime: string
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}> = [
  {
    id: 'tenant-gdo-1',
    tenantId: 'tenant-123',
    dayOfWeek: 2,
    localStartTime: '20:00:00',
    isActive: true,
    createdAt: '2026-05-01T10:00:00.000Z',
    updatedAt: null,
  },
]

const goalkeeperPlayerIds = new Set(['player-1'])

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
    const requestClone = request.clone()

    let body: {
      name: string
      slug: string
      logo: File | null
      street: string
      number: string
      neighborhood: string
      city: string
      state: string
      zipCode: string
      associationLatitude: number
      associationLongitude: number
      adminEmail: string
      adminPassword: string
    }

    try {
      const formData = await request.formData()
      body = {
        name: String(formData.get('Name') ?? ''),
        slug: String(formData.get('Slug') ?? ''),
        logo: formData.get('Logo') as File | null,
        street: String(formData.get('Street') ?? ''),
        number: String(formData.get('Number') ?? ''),
        neighborhood: String(formData.get('Neighborhood') ?? ''),
        city: String(formData.get('City') ?? ''),
        state: String(formData.get('State') ?? ''),
        zipCode: String(formData.get('ZipCode') ?? ''),
        associationLatitude: Number(formData.get('AssociationLatitude') ?? NaN),
        associationLongitude: Number(formData.get('AssociationLongitude') ?? NaN),
        adminEmail: String(formData.get('AdminEmail') ?? ''),
        adminPassword: String(formData.get('AdminPassword') ?? ''),
      }
    } catch {
      const rawBody = await requestClone.text().catch(() => '')
      const getHeaderValue = (name: string): string => request.headers.get(name) ?? ''
      const getMultipartValue = (field: string): string => {
        const escapedField = field.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        const pattern = new RegExp(`name=\\"${escapedField}\\"\\r?\\n\\r?\\n([^\\r\\n]*)`, 'i')
        const match = rawBody.match(pattern)
        return match?.[1] ?? ''
      }

      body = {
        name: getMultipartValue('Name') || getHeaderValue('X-Association-Name'),
        slug: getMultipartValue('Slug') || getHeaderValue('X-Association-Slug'),
        logo: new File(['mock'], 'logo.png', { type: 'image/png' }),
        street: getMultipartValue('Street') || getHeaderValue('X-Association-Street'),
        number: getMultipartValue('Number') || getHeaderValue('X-Association-Number'),
        neighborhood: getMultipartValue('Neighborhood') || getHeaderValue('X-Association-Neighborhood'),
        city: getMultipartValue('City') || getHeaderValue('X-Association-City'),
        state: getMultipartValue('State') || getHeaderValue('X-Association-State'),
        zipCode: getMultipartValue('ZipCode') || getHeaderValue('X-Association-ZipCode'),
        associationLatitude: Number(
          getMultipartValue('AssociationLatitude') || getHeaderValue('X-Association-Latitude') || NaN,
        ),
        associationLongitude: Number(
          getMultipartValue('AssociationLongitude') || getHeaderValue('X-Association-Longitude') || NaN,
        ),
        adminEmail: getMultipartValue('AdminEmail') || getHeaderValue('X-Association-AdminEmail'),
        adminPassword: getMultipartValue('AdminPassword') || getHeaderValue('X-Association-AdminPassword'),
      }
    }

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

    if (!body.logo) {
      return HttpResponse.json(
        { title: 'TENANT_LOGO_REQUIRED', detail: 'Logo is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.street) {
      return HttpResponse.json(
        { title: 'TENANT_STREET_REQUIRED', detail: 'Street is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.number) {
      return HttpResponse.json(
        { title: 'TENANT_NUMBER_REQUIRED', detail: 'Number is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.city) {
      return HttpResponse.json(
        { title: 'TENANT_CITY_REQUIRED', detail: 'City is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.state) {
      return HttpResponse.json(
        { title: 'TENANT_STATE_REQUIRED', detail: 'State is required', status: 422 },
        { status: 422 },
      )
    }

    if (!body.zipCode) {
      return HttpResponse.json(
        { title: 'TENANT_ZIPCODE_REQUIRED', detail: 'ZipCode is required', status: 422 },
        { status: 422 },
      )
    }

    if (!Number.isFinite(body.associationLatitude) || body.associationLatitude < -90 || body.associationLatitude > 90) {
      return HttpResponse.json(
        {
          title: 'TENANT_ASSOCIATION_LATITUDE_INVALID',
          detail: 'Association latitude must be between -90 and 90',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (!Number.isFinite(body.associationLongitude) || body.associationLongitude < -180 || body.associationLongitude > 180) {
      return HttpResponse.json(
        {
          title: 'TENANT_ASSOCIATION_LONGITUDE_INVALID',
          detail: 'Association longitude must be between -180 and 180',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (!body.adminEmail || !body.adminPassword) {
      return HttpResponse.json(
        {
          title: 'TENANT_ADMIN_CREDENTIALS_REQUIRED',
          detail: 'Admin credentials are required',
          status: 422,
        },
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
      playersPerTeam: 11,
      logoPath: 'tenant-logos/tenant-123/logo.png',
      street: body.street,
      number: body.number,
      neighborhood: body.neighborhood || null,
      city: body.city,
      state: body.state,
      zipCode: body.zipCode,
      associationLatitude: body.associationLatitude,
      associationLongitude: body.associationLongitude,
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
      playersPerTeam: 11,
      logoPath: 'tenant-logos/tenant-123/logo.png',
      street: 'Rua das Palmeiras',
      number: '123',
      neighborhood: 'Centro',
      city: 'Sao Paulo',
      state: 'SP',
      zipCode: '01000-000',
      associationLatitude: -23.5505,
      associationLongitude: -46.6333,
    }

    return HttpResponse.json(response)
  }),

  // GET /api/v1/tenant/settings
  http.get(`${BASE_URL}/api/v1/tenant/settings`, () => {
    const response: TenantResponse = {
      id: 'tenant-123',
      name: 'Mock Tenant',
      slug: 'mock-tenant',
      provisioningStatus: 'Ready',
      playersPerTeam: 11,
      logoPath: 'tenant-logos/tenant-123/logo.png',
      street: 'Rua das Palmeiras',
      number: '123',
      neighborhood: 'Centro',
      city: 'Sao Paulo',
      state: 'SP',
      zipCode: '01000-000',
      associationLatitude: -23.5505,
      associationLongitude: -46.6333,
    }

    return HttpResponse.json(response)
  }),

  // PUT /api/v1/tenant/settings
  http.put(`${BASE_URL}/api/v1/tenant/settings`, async ({ request }) => {
    const requestClone = request.clone()

    let name: string
    let street: string
    let number: string
    let neighborhood: string
    let city: string
    let state: string
    let zipCode: string
    let associationLatitude: number
    let associationLongitude: number
    let playersPerTeam: number

    try {
      const formData = await request.formData()
      name = String(formData.get('Name') ?? '')
      playersPerTeam = Number(formData.get('PlayersPerTeam') ?? 0)
      street = String(formData.get('Street') ?? '')
      number = String(formData.get('Number') ?? '')
      neighborhood = String(formData.get('Neighborhood') ?? '')
      city = String(formData.get('City') ?? '')
      state = String(formData.get('State') ?? '')
      zipCode = String(formData.get('ZipCode') ?? '')
      associationLatitude = Number(formData.get('AssociationLatitude') ?? NaN)
      associationLongitude = Number(formData.get('AssociationLongitude') ?? NaN)
    } catch {
      const rawBody = await requestClone.text().catch(() => '')
      const getHeaderValue = (headerName: string): string => request.headers.get(headerName) ?? ''
      const getMultipartValue = (fieldName: string): string => {
        const escapedField = fieldName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        const pattern = new RegExp(`name=\\"${escapedField}\\"\\r?\\n\\r?\\n([^\\r\\n]*)`, 'i')
        const match = rawBody.match(pattern)
        return match?.[1] ?? ''
      }

      name = getMultipartValue('Name') || getHeaderValue('X-Tenant-Name')
      playersPerTeam = Number(getMultipartValue('PlayersPerTeam') || getHeaderValue('X-Tenant-PlayersPerTeam') || '0')
      street = getMultipartValue('Street') || getHeaderValue('X-Tenant-Street')
      number = getMultipartValue('Number') || getHeaderValue('X-Tenant-Number')
      neighborhood = getMultipartValue('Neighborhood') || getHeaderValue('X-Tenant-Neighborhood')
      city = getMultipartValue('City') || getHeaderValue('X-Tenant-City')
      state = getMultipartValue('State') || getHeaderValue('X-Tenant-State')
      zipCode = getMultipartValue('ZipCode') || getHeaderValue('X-Tenant-ZipCode')
      associationLatitude = Number(getMultipartValue('AssociationLatitude') || getHeaderValue('X-Tenant-Latitude') || NaN)
      associationLongitude = Number(getMultipartValue('AssociationLongitude') || getHeaderValue('X-Tenant-Longitude') || NaN)
    }

    if (!name) {
      return HttpResponse.json(
        { title: 'TENANT_NAME_REQUIRED', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    if (playersPerTeam <= 0) {
      return HttpResponse.json(
        {
          title: 'TENANT_PLAYERS_PER_TEAM_INVALID',
          detail: 'PlayersPerTeam must be greater than zero',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (!Number.isFinite(associationLatitude) || associationLatitude < -90 || associationLatitude > 90) {
      return HttpResponse.json(
        {
          title: 'TENANT_ASSOCIATION_LATITUDE_INVALID',
          detail: 'Association latitude must be between -90 and 90',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (!Number.isFinite(associationLongitude) || associationLongitude < -180 || associationLongitude > 180) {
      return HttpResponse.json(
        {
          title: 'TENANT_ASSOCIATION_LONGITUDE_INVALID',
          detail: 'Association longitude must be between -180 and 180',
          status: 422,
        },
        { status: 422 },
      )
    }

    const response: TenantResponse = {
      id: 'tenant-123',
      name,
      slug: 'mock-tenant',
      provisioningStatus: 'Ready',
      playersPerTeam,
      logoPath: 'tenant-logos/tenant-123/new-logo.png',
      street,
      number,
      neighborhood: neighborhood || null,
      city,
      state,
      zipCode,
      associationLatitude,
      associationLongitude,
    }

    return HttpResponse.json(response)
  }),

  // GET /api/v1/tenant/settings/game-day-options
  http.get(`${BASE_URL}/api/v1/tenant/settings/game-day-options`, () => {
    return HttpResponse.json(mockTenantGameDayOptions)
  }),

  // POST /api/v1/tenant/settings/game-day-options
  http.post(`${BASE_URL}/api/v1/tenant/settings/game-day-options`, async ({ request }) => {
    const body = (await request.json()) as {
      dayOfWeek: number
      localStartTime: string
    }

    if (
      mockTenantGameDayOptions.some(
        (item) => item.isActive && item.dayOfWeek === body.dayOfWeek && item.localStartTime === body.localStartTime,
      )
    ) {
      return HttpResponse.json(
        {
          title: 'TENANT_GAMEDAY_OPTION_ALREADY_EXISTS',
          detail: 'An active option with the same day and time already exists.',
          status: 409,
        },
        { status: 409 },
      )
    }

    const created = {
      id: `tenant-gdo-${crypto.randomUUID()}`,
      tenantId: 'tenant-123',
      dayOfWeek: body.dayOfWeek,
      localStartTime: body.localStartTime,
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: null,
    }

    mockTenantGameDayOptions = [...mockTenantGameDayOptions, created]

    return HttpResponse.json(created, { status: 201 })
  }),

  // PUT /api/v1/tenant/settings/game-day-options/:id/status
  http.put(`${BASE_URL}/api/v1/tenant/settings/game-day-options/:id/status`, async ({ params, request }) => {
    const id = String(params.id)
    const body = (await request.json()) as { isActive: boolean }

    const existing = mockTenantGameDayOptions.find((item) => item.id === id)
    if (!existing) {
      return HttpResponse.json(
        {
          title: 'TENANT_GAMEDAY_OPTION_NOT_FOUND',
          detail: 'Game day option was not found.',
          status: 404,
        },
        { status: 404 },
      )
    }

    if (body.isActive) {
      const duplicate = mockTenantGameDayOptions.some(
        (item) =>
          item.id !== existing.id &&
          item.isActive &&
          item.dayOfWeek === existing.dayOfWeek &&
          item.localStartTime === existing.localStartTime,
      )

      if (duplicate) {
        return HttpResponse.json(
          {
            title: 'TENANT_GAMEDAY_OPTION_ALREADY_EXISTS',
            detail: 'An active option with the same day and time already exists.',
            status: 409,
          },
          { status: 409 },
        )
      }
    }

    mockTenantGameDayOptions = mockTenantGameDayOptions.map((item) =>
      item.id === id
        ? {
            ...item,
            isActive: body.isActive,
            updatedAt: new Date().toISOString(),
          }
        : item,
    )

    return HttpResponse.json(mockTenantGameDayOptions.find((item) => item.id === id))
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

    if (!Array.isArray(body.positionIds)) {
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

  // POST /api/v1/position
  http.post(`${BASE_URL}/api/v1/position`, async ({ request }) => {
    const body = (await request.json()) as {
      code?: string
      name?: string
      description?: string | null
    }

    const code = body.code?.trim() ?? ''
    const name = body.name?.trim() ?? ''

    if (code.length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_CODE', detail: 'Code is required', status: 422 },
        { status: 422 },
      )
    }

    if (name.length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_NAME', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    const duplicated = mockPositions.some(
      (item) => item.code.trim().toLowerCase() === code.toLowerCase(),
    )

    if (duplicated) {
      return HttpResponse.json(
        { title: 'POSITION_ALREADY_EXISTS', detail: 'Position already exists', status: 409 },
        { status: 409 },
      )
    }

    const created = {
      id: `position-new-${Date.now()}`,
      tenantId: 'tenant-123',
      code,
      name,
      description: body.description?.trim() ? body.description.trim() : null,
      isActive: true,
      createdAt: new Date().toISOString(),
    }

    mockPositions = [...mockPositions, created]

    return HttpResponse.json(created, { status: 201 })
  }),

  // PUT /api/v1/position/:id
  http.put(`${BASE_URL}/api/v1/position/:id`, async ({ params, request }) => {
    const target = mockPositions.find((item) => item.id === params.id)

    if (!target) {
      return HttpResponse.json(
        { title: 'POSITION_NOT_FOUND', detail: 'Position not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as {
      code?: string
      name?: string
      description?: string | null
    }

    const code = body.code?.trim() ?? ''
    const name = body.name?.trim() ?? ''

    if (code.length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_CODE', detail: 'Code is required', status: 422 },
        { status: 422 },
      )
    }

    if (name.length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_NAME', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    const duplicated = mockPositions.some(
      (item) => item.id !== target.id && item.code.trim().toLowerCase() === code.toLowerCase(),
    )

    if (duplicated) {
      return HttpResponse.json(
        { title: 'POSITION_ALREADY_EXISTS', detail: 'Position already exists', status: 409 },
        { status: 409 },
      )
    }

    const updated = {
      ...target,
      code,
      name,
      description: body.description?.trim() ? body.description.trim() : null,
    }

    mockPositions = mockPositions.map((item) => (item.id === target.id ? updated : item))

    return HttpResponse.json(updated)
  }),

  // DELETE /api/v1/position/:id
  http.delete(`${BASE_URL}/api/v1/position/:id`, ({ params }) => {
    const target = mockPositions.find((item) => item.id === params.id)

    if (!target) {
      return HttpResponse.json(
        { title: 'POSITION_NOT_FOUND', detail: 'Position not found', status: 404 },
        { status: 404 },
      )
    }

    if (target.id === 'position-1') {
      return HttpResponse.json(
        { title: 'POSITION_IN_USE', detail: 'Position is in use', status: 409 },
        { status: 409 },
      )
    }

    mockPositions = mockPositions.filter((item) => item.id !== target.id)
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/team
  http.get(`${BASE_URL}/api/v1/team`, () => HttpResponse.json(mockTeams)),

  // GET /api/v1/team/:id
  http.get(`${BASE_URL}/api/v1/team/:id`, ({ params }) => {
    const team = mockTeams.find((item) => item.id === params.id)

    if (!team) {
      return HttpResponse.json(
        { title: 'TEAM_NOT_FOUND', detail: 'Team not found', status: 404 },
        { status: 404 },
      )
    }

    return HttpResponse.json(team)
  }),

  // POST /api/v1/team
  http.post(`${BASE_URL}/api/v1/team`, async ({ request }) => {
    const body = (await request.json()) as {
      name: string
      maxPlayers: number
    }

    if (!body.name || body.name.trim().length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_NAME', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    if (!Number.isInteger(body.maxPlayers) || body.maxPlayers <= 0) {
      return HttpResponse.json(
        {
          title: 'INVALID_MAX_PLAYERS',
          detail: 'Max players must be greater than zero',
          status: 422,
        },
        { status: 422 },
      )
    }

    const duplicated = mockTeams.some(
      (item) => item.name.toLowerCase() === body.name.trim().toLowerCase(),
    )

    if (duplicated) {
      return HttpResponse.json(
        { title: 'TEAM_ALREADY_EXISTS', detail: 'Team already exists', status: 409 },
        { status: 409 },
      )
    }

    return HttpResponse.json(
      {
        id: 'team-new',
        tenantId: 'tenant-123',
        name: body.name.trim(),
        maxPlayers: body.maxPlayers,
        isActive: true,
        createdAt: '2026-05-05T10:00:00.000Z',
        playerIds: [],
      },
      { status: 201 },
    )
  }),

  // PUT /api/v1/team/:id
  http.put(`${BASE_URL}/api/v1/team/:id`, async ({ params, request }) => {
    const team = mockTeams.find((item) => item.id === params.id)

    if (!team) {
      return HttpResponse.json(
        { title: 'TEAM_NOT_FOUND', detail: 'Team not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as {
      name: string
      maxPlayers: number
    }

    if (!body.name || body.name.trim().length === 0) {
      return HttpResponse.json(
        { title: 'INVALID_NAME', detail: 'Name is required', status: 422 },
        { status: 422 },
      )
    }

    if (!Number.isInteger(body.maxPlayers) || body.maxPlayers <= 0) {
      return HttpResponse.json(
        {
          title: 'INVALID_MAX_PLAYERS',
          detail: 'Max players must be greater than zero',
          status: 422,
        },
        { status: 422 },
      )
    }

    const duplicated = mockTeams.some(
      (item) =>
        item.id !== params.id && item.name.toLowerCase() === body.name.trim().toLowerCase(),
    )

    if (duplicated) {
      return HttpResponse.json(
        { title: 'TEAM_ALREADY_EXISTS', detail: 'Team already exists', status: 409 },
        { status: 409 },
      )
    }

    return HttpResponse.json({
      ...team,
      name: body.name.trim(),
      maxPlayers: body.maxPlayers,
    })
  }),

  // PUT /api/v1/team/:id/players
  http.put(`${BASE_URL}/api/v1/team/:id/players`, async ({ params, request }) => {
    const team = mockTeams.find((item) => item.id === params.id)

    if (!team) {
      return HttpResponse.json(
        { title: 'TEAM_NOT_FOUND', detail: 'Team not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as { playerIds: string[] }

    if (!Array.isArray(body.playerIds)) {
      return HttpResponse.json(
        { title: 'TEAM_INVALID_PLAYER_ID', detail: 'PlayerIds invalid', status: 422 },
        { status: 422 },
      )
    }

    const hasInvalidId = body.playerIds.some((playerId) => !playerId || playerId.trim().length === 0)
    if (hasInvalidId) {
      return HttpResponse.json(
        { title: 'TEAM_INVALID_PLAYER_ID', detail: 'PlayerIds invalid', status: 422 },
        { status: 422 },
      )
    }

    if (new Set(body.playerIds).size !== body.playerIds.length) {
      return HttpResponse.json(
        { title: 'TEAM_DUPLICATE_PLAYERS', detail: 'Duplicate players', status: 422 },
        { status: 422 },
      )
    }

    if (body.playerIds.length > team.maxPlayers) {
      return HttpResponse.json(
        {
          title: 'TEAM_PLAYERS_LIMIT_EXCEEDED',
          detail: 'Team players limit exceeded',
          status: 422,
        },
        { status: 422 },
      )
    }

    const hasUnknownOrInactivePlayer = body.playerIds.some((playerId) => {
      const player = mockPlayers.find((item) => item.id === playerId)
      return !player || !player.isActive
    })

    if (hasUnknownOrInactivePlayer) {
      return HttpResponse.json(
        { title: 'TEAM_PLAYER_NOT_FOUND', detail: 'Player not found', status: 404 },
        { status: 404 },
      )
    }

    const hasGoalkeeper = body.playerIds.some((playerId) => goalkeeperPlayerIds.has(playerId))
    if (body.playerIds.length > 0 && !hasGoalkeeper) {
      return HttpResponse.json(
        {
          title: 'TEAM_GOALKEEPER_REQUIRED',
          detail: 'At least one goalkeeper is required',
          status: 422,
        },
        { status: 422 },
      )
    }

    return HttpResponse.json({
      teamId: params.id,
      playerIds: body.playerIds,
      updatedAt: '2026-05-05T10:10:00.000Z',
    })
  }),

  // DELETE /api/v1/team/:id
  http.delete(`${BASE_URL}/api/v1/team/:id`, ({ params }) => {
    const team = mockTeams.find((item) => item.id === params.id)

    if (!team) {
      return HttpResponse.json(
        { title: 'TEAM_NOT_FOUND', detail: 'Team not found', status: 404 },
        { status: 404 },
      )
    }

    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/gameday
  http.get(`${BASE_URL}/api/v1/gameday`, () => HttpResponse.json(mockGameDays)),

  // GET /api/v1/match
  http.get(`${BASE_URL}/api/v1/match`, ({ request }) => {
    const url = new URL(request.url)
    const status = url.searchParams.get('status')

    if (!status) {
      return HttpResponse.json(mockMatches)
    }

    return HttpResponse.json(mockMatches.filter((item) => item.status === status))
  }),

  // GET /api/v1/match/:id
  http.get(`${BASE_URL}/api/v1/match/:id`, ({ params }) => {
    const match = mockMatches.find((item) => item.id === params.id)

    if (!match) {
      return HttpResponse.json(
        { title: 'MATCH_NOT_FOUND', detail: 'Match not found', status: 404 },
        { status: 404 },
      )
    }

    return HttpResponse.json(match)
  }),

  // POST /api/v1/match
  http.post(`${BASE_URL}/api/v1/match`, async ({ request }) => {
    const body = (await request.json()) as {
      gameDayId: string
      homeTeamId: string
      awayTeamId: string
      description?: string | null
    }

    if (body.homeTeamId === body.awayTeamId) {
      return HttpResponse.json(
        {
          title: 'TEAMS_MUST_BE_DIFFERENT',
          detail: 'Home and away teams must be different',
          status: 422,
        },
        { status: 422 },
      )
    }

    const gameDay = mockGameDays.find((item) => item.id === body.gameDayId)
    if (!gameDay) {
      return HttpResponse.json(
        { title: 'GAMEDAY_NOT_FOUND', detail: 'Game day not found', status: 404 },
        { status: 404 },
      )
    }

    if (body.gameDayId === 'gameday-past') {
      return HttpResponse.json(
        { title: 'GAMEDAY_PAST', detail: 'Cannot create match for past game day', status: 422 },
        { status: 422 },
      )
    }

    const hasHome = mockTeams.some((item) => item.id === body.homeTeamId)
    const hasAway = mockTeams.some((item) => item.id === body.awayTeamId)
    if (!hasHome || !hasAway) {
      return HttpResponse.json(
        { title: 'TEAM_NOT_FOUND', detail: 'Team not found', status: 404 },
        { status: 404 },
      )
    }

    const alreadyExists = mockMatches.some(
      (item) =>
        item.gameDayId === body.gameDayId &&
        ((item.homeTeamId === body.homeTeamId && item.awayTeamId === body.awayTeamId) ||
          (item.homeTeamId === body.awayTeamId && item.awayTeamId === body.homeTeamId)),
    )

    if (alreadyExists) {
      return HttpResponse.json(
        { title: 'MATCH_ALREADY_EXISTS', detail: 'Match already exists', status: 409 },
        { status: 409 },
      )
    }

    return HttpResponse.json(
      {
        id: 'match-new',
        tenantId: 'tenant-123',
        gameDayId: body.gameDayId,
        homeTeamId: body.homeTeamId,
        awayTeamId: body.awayTeamId,
        description: body.description ?? null,
        status: 'Pending',
        isActive: true,
        createdAt: new Date().toISOString(),
      },
      { status: 201 },
    )
  }),

  // PUT /api/v1/match/:id
  http.put(`${BASE_URL}/api/v1/match/:id`, async ({ params, request }) => {
    const match = mockMatches.find((item) => item.id === params.id)

    if (!match) {
      return HttpResponse.json(
        { title: 'MATCH_NOT_FOUND', detail: 'Match not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as {
      gameDayId: string
      homeTeamId: string
      awayTeamId: string
      description?: string | null
    }

    if (body.homeTeamId === body.awayTeamId) {
      return HttpResponse.json(
        {
          title: 'TEAMS_MUST_BE_DIFFERENT',
          detail: 'Home and away teams must be different',
          status: 422,
        },
        { status: 422 },
      )
    }

    if (body.gameDayId === 'gameday-past') {
      return HttpResponse.json(
        { title: 'GAMEDAY_PAST', detail: 'Cannot update match for past game day', status: 422 },
        { status: 422 },
      )
    }

    return HttpResponse.json({
      ...match,
      gameDayId: body.gameDayId,
      homeTeamId: body.homeTeamId,
      awayTeamId: body.awayTeamId,
      description: body.description ?? null,
    })
  }),

  // PUT /api/v1/match/:id/status
  http.put(`${BASE_URL}/api/v1/match/:id/status`, async ({ params, request }) => {
    const match = mockMatches.find((item) => item.id === params.id)

    if (!match) {
      return HttpResponse.json(
        { title: 'MATCH_NOT_FOUND', detail: 'Match not found', status: 404 },
        { status: 404 },
      )
    }

    const body = (await request.json()) as { status: string }

    const allowedTransitions: Record<string, string[]> = {
      Pending: ['Scheduled', 'Cancelled'],
      Scheduled: ['InProgress', 'Cancelled'],
      InProgress: ['Completed'],
      Completed: [],
      Cancelled: [],
    }

    const canTransition = allowedTransitions[match.status]?.includes(body.status)
    if (!canTransition && body.status !== match.status) {
      return HttpResponse.json(
        {
          title: 'INVALID_STATUS_TRANSITION',
          detail: 'Invalid match status transition',
          status: 422,
        },
        { status: 422 },
      )
    }

    return HttpResponse.json({
      ...match,
      status: body.status,
    })
  }),

  // DELETE /api/v1/match/:id
  http.delete(`${BASE_URL}/api/v1/match/:id`, ({ params }) => {
    const match = mockMatches.find((item) => item.id === params.id)

    if (!match) {
      return HttpResponse.json(
        { title: 'MATCH_NOT_FOUND', detail: 'Match not found', status: 404 },
        { status: 404 },
      )
    }

    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/checkin/gameday/:gameDayId
  http.get(`${BASE_URL}/api/v1/checkin/gameday/:gameDayId`, ({ params }) => {
    const { gameDayId } = params

    return HttpResponse.json(mockCheckins.filter((item) => item.gameDayId === gameDayId))
  }),

  // GET /api/v1/checkin/player/:playerId
  http.get(`${BASE_URL}/api/v1/checkin/player/:playerId`, ({ params }) => {
    const { playerId } = params

    return HttpResponse.json(mockCheckins.filter((item) => item.playerId === playerId))
  }),

  // POST /api/v1/checkin
  http.post(`${BASE_URL}/api/v1/checkin`, async ({ request }) => {
    const body = (await request.json()) as {
      playerId: string
      gameDayId: string
      checkedInAtUtc: string
      latitude: number
      longitude: number
    }

    if (body.playerId === 'player-missing') {
      return HttpResponse.json(
        { title: 'PLAYER_NOT_FOUND', detail: 'Player not found', status: 404 },
        { status: 404 },
      )
    }

    if (body.playerId === 'player-3') {
      return HttpResponse.json(
        { title: 'PLAYER_INACTIVE', detail: 'Player is inactive', status: 422 },
        { status: 422 },
      )
    }

    if (body.gameDayId === 'gameday-missing') {
      return HttpResponse.json(
        { title: 'GAMEDAY_NOT_FOUND', detail: 'Game day not found', status: 404 },
        { status: 404 },
      )
    }

    if (body.playerId === 'player-1' && body.gameDayId === 'gameday-1') {
      return HttpResponse.json(
        {
          title: 'CHECKIN_ALREADY_EXISTS',
          detail: 'A check-in already exists for this player and game day',
          status: 409,
        },
        { status: 409 },
      )
    }

    if (Math.abs(body.latitude) > 80 || Math.abs(body.longitude) > 170) {
      return HttpResponse.json(
        {
          title: 'CHECKIN_OUTSIDE_ALLOWED_RADIUS',
          detail: 'Player is outside association radius',
          status: 422,
        },
        { status: 422 },
      )
    }

    return HttpResponse.json(
      {
        id: 'checkin-new',
        tenantId: 'tenant-123',
        playerId: body.playerId,
        gameDayId: body.gameDayId,
        checkedInAtUtc: body.checkedInAtUtc,
        latitude: body.latitude,
        longitude: body.longitude,
        distanceFromAssociationMeters: 35.7,
        isActive: true,
        createdAt: body.checkedInAtUtc,
        cancelledAtUtc: null,
      },
      { status: 201 },
    )
  }),

  // DELETE /api/v1/checkin/:id
  http.delete(`${BASE_URL}/api/v1/checkin/:id`, ({ params }) => {
    if (params.id === 'checkin-missing') {
      return HttpResponse.json(
        { title: 'CHECKIN_NOT_FOUND', detail: 'Checkin not found', status: 404 },
        { status: 404 },
      )
    }

    return new HttpResponse(null, { status: 204 })
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
