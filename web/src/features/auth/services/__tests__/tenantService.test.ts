import { describe, it, expect } from 'vitest'
import { TENANT_HEADER_NAME } from '../tenantService'

describe('tenantService', () => {
  describe('TENANT_HEADER_NAME', () => {
    it('should equal X-Tenant-Slug', () => {
      expect(TENANT_HEADER_NAME).toBe('X-Tenant-Slug')
    })
  })
})
