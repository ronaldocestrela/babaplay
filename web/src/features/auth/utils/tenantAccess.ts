import type { TenantContext, UserProfile } from '@/features/auth/types'

export function isTenantAdmin(user: UserProfile | null, tenant: TenantContext | null): boolean {
  if (!user) return false

  const memberships = user.tenants ?? []

  if (tenant?.slug) {
    const match = memberships.find((item) => item.slug === tenant.slug)
    if (match) return match.isOwner
  }

  if (user.primaryTenant) {
    return user.primaryTenant.isOwner
  }

  return false
}
