import { describe, expect, it } from 'vitest'
import { isTenantAdmin } from '../tenantAccess'
import type { TenantContext, UserProfile } from '@/features/auth/types'

function buildUser(overrides?: Partial<UserProfile>): UserProfile {
  return {
    id: 'user-1',
    email: 'user@test.com',
    roles: ['Player'],
    isActive: true,
    createdAt: '2026-01-01T00:00:00.000Z',
    tenants: [
      {
        id: 'tenant-1',
        name: 'Tenant 1',
        slug: 'tenant-1',
        isOwner: true,
        joinedAt: '2026-01-01T00:00:00.000Z',
      },
    ],
    primaryTenant: {
      id: 'tenant-1',
      name: 'Tenant 1',
      slug: 'tenant-1',
      isOwner: true,
      joinedAt: '2026-01-01T00:00:00.000Z',
    },
    ...overrides,
  }
}

const tenant: TenantContext = { slug: 'tenant-1', source: 'query' }

describe('tenantAccess.isTenantAdmin', () => {
  it('retorna false quando usuário é nulo', () => {
    expect(isTenantAdmin(null, tenant)).toBe(false)
  })

  it('retorna isOwner do membership quando tenant da URL confere', () => {
    const user = buildUser({
      tenants: [
        {
          id: 'tenant-1',
          name: 'Tenant 1',
          slug: 'tenant-1',
          isOwner: false,
          joinedAt: '2026-01-01T00:00:00.000Z',
        },
      ],
    })

    expect(isTenantAdmin(user, tenant)).toBe(false)
  })

  it('usa primaryTenant quando tenant da URL não confere', () => {
    const user = buildUser({
      primaryTenant: {
        id: 'tenant-2',
        name: 'Tenant 2',
        slug: 'tenant-2',
        isOwner: true,
        joinedAt: '2026-01-01T00:00:00.000Z',
      },
    })

    expect(isTenantAdmin(user, { slug: 'another-tenant', source: 'query' })).toBe(true)
  })

  it('retorna false quando não há primaryTenant e não há match de membership', () => {
    const user = buildUser({
      tenants: [],
      primaryTenant: null,
    })

    expect(isTenantAdmin(user, { slug: 'tenant-x', source: 'query' })).toBe(false)
  })

  it('retorna isOwner de primaryTenant quando tenant é nulo', () => {
    const user = buildUser({
      primaryTenant: {
        id: 'tenant-1',
        name: 'Tenant 1',
        slug: 'tenant-1',
        isOwner: false,
        joinedAt: '2026-01-01T00:00:00.000Z',
      },
    })

    expect(isTenantAdmin(user, null)).toBe(false)
  })
})
