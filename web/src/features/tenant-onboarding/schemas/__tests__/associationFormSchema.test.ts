import { describe, expect, it } from 'vitest'
import { associationFormSchema } from '../associationFormSchema'

describe('associationFormSchema', () => {
  it('deve validar payload válido', () => {
    const parsed = associationFormSchema.parse({
      name: 'Associação do Bairro',
      slug: 'associacao-do-bairro',
      adminEmail: 'admin@bairro.com',
      adminPassword: 'Admin1234',
      confirmAdminPassword: 'Admin1234',
    })

    expect(parsed.name).toBe('Associação do Bairro')
    expect(parsed.slug).toBe('associacao-do-bairro')
  })

  it('deve falhar com nome vazio', () => {
    expect(() =>
      associationFormSchema.parse({
        name: '   ',
        slug: 'clube-valido',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Nome da associação é obrigatório')
  })

  it('deve falhar com slug vazio', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: '   ',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Slug é obrigatório')
  })

  it('deve falhar com slug contendo caracteres inválidos', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'Slug Invalido',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Slug deve conter apenas letras minúsculas, números e hífens')
  })

  it('deve falhar quando senhas do admin não conferem', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'clube-a',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin12345',
      }),
    ).toThrow('As senhas do admin devem ser iguais')
  })
})
