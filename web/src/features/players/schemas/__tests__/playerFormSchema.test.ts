import { describe, expect, it } from 'vitest'
import { playerFormSchema } from '../playerFormSchema'

describe('playerFormSchema', () => {
  it('deve validar payload válido', () => {
    const parsed = playerFormSchema.parse({
      userId: '2b6c6402-bb43-4945-bf4f-7df5b91b0a9e',
      name: 'Joao Silva',
      nickname: 'JS10',
      phone: '11999990001',
      dateOfBirth: '1995-10-01',
      positionIds: ['425cb75f-cf2f-44ec-8682-a94ea8018f5b'],
    })

    expect(parsed.name).toBe('Joao Silva')
    expect(parsed.positionIds).toHaveLength(1)
  })

  it('deve falhar com nome vazio', () => {
    expect(() =>
      playerFormSchema.parse({
        name: '   ',
        positionIds: [],
      }),
    ).toThrow('Nome é obrigatório')
  })

  it('deve falhar com data inválida', () => {
    expect(() =>
      playerFormSchema.parse({
        name: 'Pedro',
        dateOfBirth: '10/10/1990',
        positionIds: [],
      }),
    ).toThrow('Data de nascimento inválida')
  })

  it('deve falhar com mais de 3 posições', () => {
    expect(() =>
      playerFormSchema.parse({
        name: 'Carlos',
        positionIds: [
          '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
          'e0632a52-44de-45c7-9f6d-b6ba2e2e5ca8',
          'ac84072d-e74d-4740-9438-8f4c95bdd6f5',
          '4afc8e16-b06e-42bc-bdc6-9e9767c0f8d0',
        ],
      }),
    ).toThrow('Máximo de 3 posições por jogador')
  })
})