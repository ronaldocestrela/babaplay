import { describe, expect, it } from 'vitest'
import { associationService } from '../associationService'

describe('associationService', () => {
  it('deve criar associação com payload válido', async () => {
    const result = await associationService.createAssociation({
      name: 'Clube Verde',
      slug: 'clube-verde',
    })

    expect(result.id).toBe('tenant-123')
    expect(result.name).toBe('Clube Verde')
    expect(result.slug).toBe('clube-verde')
    expect(result.provisioningStatus).toBe('Pending')
  })

  it('deve lançar erro 409 para slug já em uso', async () => {
    await expect(
      associationService.createAssociation({
        name: 'Clube Duplicado',
        slug: 'taken-slug',
      }),
    ).rejects.toMatchObject({ response: { status: 409 } })
  })

  it('deve consultar status da associação', async () => {
    const result = await associationService.getAssociationStatus('tenant-123')

    expect(result.id).toBe('tenant-123')
    expect(result.provisioningStatus).toBe('Ready')
  })

  it('deve lançar erro 404 ao consultar associação inexistente', async () => {
    await expect(associationService.getAssociationStatus('unknown-tenant-id')).rejects.toMatchObject({
      response: { status: 404 },
    })
  })
})
