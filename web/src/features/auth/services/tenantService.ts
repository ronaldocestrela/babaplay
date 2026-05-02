import type { TenantContext } from '../types'

/** Header name sent to the API to identify the active tenant. */
export const TENANT_HEADER_NAME = 'X-Tenant-Slug'

/**
 * Extracts the tenant slug from a hostname, assuming the convention:
 *   <slug>.<domain>   →  slug
 *   <domain>          →  null  (no subdomain or www)
 *
 * Examples:
 *   "myclob.babaplay.app"  → "myclob"
 *   "babaplay.app"         → null
 *   "www.babaplay.app"     → null
 *   "localhost"            → null
 */
export function parseTenantSlug(hostname: string): string | null {
  const parts = hostname.split('.')

  // At least 3 parts required: subdomain + domain + tld
  if (parts.length < 3) return null

  const subdomain = parts[0].toLowerCase()
  // Ignore common non-tenant subdomains
  if (!subdomain || subdomain === 'www' || subdomain === 'app') return null

  return subdomain
}

/**
 * Resolves the active tenant for the current browser location.
 *
 * Priority:
 *   1. Subdomain  (production: myclob.babaplay.app)
 *   2. `?tenant=` query parameter  (local dev: localhost:5173?tenant=myclob)
 *
 * Returns `null` when no tenant can be determined (e.g. the root landing page).
 */
export function getTenantFromUrl(): TenantContext | null {
  if (typeof window === 'undefined') return null

  const { hostname, search } = window.location

  // 1. Try subdomain
  const slug = parseTenantSlug(hostname)
  if (slug) {
    return { slug, source: 'subdomain' }
  }

  // 2. Fallback: ?tenant= query param (local dev)
  const params = new URLSearchParams(search)
  const querySlug = params.get('tenant')?.trim().toLowerCase()
  if (querySlug) {
    return { slug: querySlug, source: 'query' }
  }

  return null
}
