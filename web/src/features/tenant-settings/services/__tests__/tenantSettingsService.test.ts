import { describe, expect, it } from 'vitest'
import { tenantSettingsService } from '../tenantSettingsService'

describe('tenantSettingsService', () => {
  it('deve carregar configurações do tenant', async () => {
    const result = await tenantSettingsService.getSettings()

    expect(result.id).toBe('tenant-123')
    expect(result.name).toBe('Mock Tenant')
    expect(result.city).toBe('Sao Paulo')
  })

  it('deve atualizar configurações do tenant', async () => {
    const logo = new File(['fake-image'], 'logo.png', { type: 'image/png' })

    const result = await tenantSettingsService.updateSettings({
      name: 'Tenant Atualizado',
      logo,
      street: 'Rua Nova',
      number: '99',
      neighborhood: 'Centro',
      city: 'Campinas',
      state: 'SP',
      zipCode: '13000-000',
    })

    expect(result.name).toBe('Tenant Atualizado')
    expect(result.street).toBe('Rua Nova')
    expect(result.number).toBe('99')
    expect(result.city).toBe('Campinas')
    expect(result.state).toBe('SP')
    expect(result.zipCode).toBe('13000-000')
    expect(result.logoPath).toContain('tenant-logos/')
  })
})
