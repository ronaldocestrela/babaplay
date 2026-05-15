import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { parseTenantSlug, getTenantFromUrl, TENANT_HEADER_NAME } from '../tenantService'

describe('tenantService', () => {
  describe('TENANT_HEADER_NAME', () => {
    it('should equal X-Tenant-Slug', () => {
      expect(TENANT_HEADER_NAME).toBe('X-Tenant-Slug')
    })
  })

  describe('parseTenantSlug', () => {
    it('returns slug for a standard 3-part hostname', () => {
      expect(parseTenantSlug('myclob.babaplay.app')).toBe('myclob')
    })

    it('returns null for a 2-part hostname (no subdomain)', () => {
      expect(parseTenantSlug('babaplay.app')).toBeNull()
    })

    it('returns null for a single-part hostname like localhost', () => {
      expect(parseTenantSlug('localhost')).toBeNull()
    })

    it('returns null when subdomain is "www"', () => {
      expect(parseTenantSlug('www.babaplay.app')).toBeNull()
    })

    it('returns null when subdomain is "app"', () => {
      expect(parseTenantSlug('app.babaplay.app')).toBeNull()
    })

    it('normalises slug to lowercase', () => {
      expect(parseTenantSlug('MyClob.babaplay.app')).toBe('myclob')
    })

    it('returns slug for 4-part hostnames (e.g. tenant.sub.domain.tld)', () => {
      expect(parseTenantSlug('tenant.sub.babaplay.app')).toBe('tenant')
    })
  })

  describe('getTenantFromUrl', () => {
    const originalLocation = window.location

    const setMockLocation = (hostname: string, search: string) => {
      Object.defineProperty(window, 'location', {
        writable: true,
        value: { hostname, search } as unknown as Location,
      })
    }

    beforeEach(() => {
      setMockLocation('', '')
    })

    afterEach(() => {
      Object.defineProperty(window, 'location', {
        writable: true,
        value: originalLocation,
      })
    })

    it('returns TenantContext from subdomain when 3-part hostname present', () => {
      setMockLocation('myclob.babaplay.app', '')
      const result = getTenantFromUrl()
      expect(result).toEqual({ slug: 'myclob', source: 'subdomain' })
    })

    it('returns TenantContext from ?tenant= query param when no subdomain', () => {
      setMockLocation('localhost', '?tenant=testclub')
      const result = getTenantFromUrl()
      expect(result).toEqual({ slug: 'testclub', source: 'query' })
    })

    it('normalises ?tenant= query param value to lowercase', () => {
      setMockLocation('localhost', '?tenant=TestClub')
      const result = getTenantFromUrl()
      expect(result?.slug).toBe('testclub')
    })

    it('returns null when no subdomain and no ?tenant= param', () => {
      setMockLocation('localhost', '')
      const result = getTenantFromUrl()
      expect(result).toBeNull()
    })

    it('prefers subdomain over query param', () => {
      setMockLocation('myclob.babaplay.app', '?tenant=other')
      const result = getTenantFromUrl()
      expect(result).toEqual({ slug: 'myclob', source: 'subdomain' })
    })

    it('returns null when window is undefined (SSR guard)', () => {
      const windowSpy = vi.spyOn(globalThis, 'window', 'get')
      windowSpy.mockReturnValue(undefined as unknown as Window & typeof globalThis)
      const result = getTenantFromUrl()
      expect(result).toBeNull()
      windowSpy.mockRestore()
    })
  })
})
