import { describe, expect, it } from 'vitest'
import { associationService } from '../associationService'

describe('associationService', () => {
  const logo = new File(['logo'], 'logo.png', { type: 'image/png' })

  it('deve criar associação com payload válido', async () => {
    const result = await associationService.createAssociation({
      name: 'Clube Verde',
      slug: 'clube-verde',
      logo,
      street: 'Rua Verde',
      number: '123',
      neighborhood: 'Centro',
      city: 'Sao Paulo',
      state: 'SP',
      zipCode: '01000-000',
      associationLatitude: -23.5505,
      associationLongitude: -46.6333,
      adminEmail: 'admin@clubeverde.com',
      adminPassword: 'Admin1234',
    })

    expect(result.id).toBe('tenant-123')
    expect(result.name).toBe('Clube Verde')
    expect(result.slug).toBe('clube-verde')
  })

  it('deve lançar erro 409 para slug já em uso', async () => {
    await expect(
      associationService.createAssociation({
        name: 'Clube Duplicado',
        slug: 'taken-slug',
        logo,
        street: 'Rua Azul',
        number: '999',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: -23.5505,
        associationLongitude: -46.6333,
        adminEmail: 'admin@duplicado.com',
        adminPassword: 'Admin1234',
      }),
    ).rejects.toMatchObject({ response: { status: 409 } })
  })
})
