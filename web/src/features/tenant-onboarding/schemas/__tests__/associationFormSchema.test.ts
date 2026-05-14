import { describe, expect, it } from 'vitest'
import { associationFormSchema } from '../associationFormSchema'

describe('associationFormSchema', () => {
  const validLogo = new File(['content'], 'logo.png', { type: 'image/png' })

  it('deve validar payload válido', () => {
    const parsed = associationFormSchema.parse({
      name: 'Associação do Bairro',
      slug: 'associacao-do-bairro',
      logo: validLogo,
      street: 'Rua das Flores',
      number: '120',
      neighborhood: 'Centro',
      city: 'Sao Paulo',
      state: 'SP',
      zipCode: '01000-000',
      associationLatitude: '-23.5505',
      associationLongitude: '-46.6333',
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
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-46.6333',
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
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-46.6333',
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
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-46.6333',
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
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-46.6333',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin12345',
      }),
    ).toThrow('As senhas do admin devem ser iguais')
  })

  it('deve falhar sem logo', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'clube-a',
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-46.6333',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Logo da associação é obrigatório')
  })

  it('deve falhar com latitude inválida', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'clube-a',
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '91',
        associationLongitude: '-46.6333',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Latitude inválida')
  })

  it('deve falhar com longitude inválida', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'clube-a',
        logo: validLogo,
        street: 'Rua das Flores',
        number: '120',
        neighborhood: 'Centro',
        city: 'Sao Paulo',
        state: 'SP',
        zipCode: '01000-000',
        associationLatitude: '-23.5505',
        associationLongitude: '-181',
        adminEmail: 'admin@clube.com',
        adminPassword: 'Admin1234',
        confirmAdminPassword: 'Admin1234',
      }),
    ).toThrow('Longitude inválida')
  })
})
