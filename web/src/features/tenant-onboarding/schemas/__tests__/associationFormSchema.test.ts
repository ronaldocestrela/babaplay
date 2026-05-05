import { describe, expect, it } from 'vitest'
import { associationFormSchema } from '../associationFormSchema'

describe('associationFormSchema', () => {
  it('deve validar payload válido', () => {
    const parsed = associationFormSchema.parse({
      name: 'Associação do Bairro',
      slug: 'associacao-do-bairro',
    })

    expect(parsed.name).toBe('Associação do Bairro')
    expect(parsed.slug).toBe('associacao-do-bairro')
  })

  it('deve falhar com nome vazio', () => {
    expect(() =>
      associationFormSchema.parse({
        name: '   ',
        slug: 'clube-valido',
      }),
    ).toThrow('Nome da associação é obrigatório')
  })

  it('deve falhar com slug vazio', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: '   ',
      }),
    ).toThrow('Slug é obrigatório')
  })

  it('deve falhar com slug contendo caracteres inválidos', () => {
    expect(() =>
      associationFormSchema.parse({
        name: 'Clube A',
        slug: 'Slug Invalido',
      }),
    ).toThrow('Slug deve conter apenas letras minúsculas, números e hífens')
  })
})
