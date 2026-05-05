import { describe, expect, it } from 'vitest'
import { checkinFormSchema } from '../checkinFormSchema'

describe('checkinFormSchema', () => {
  it('deve validar payload válido', () => {
    const parsed = checkinFormSchema.parse({
      playerId: '2b6c6402-bb43-4945-bf4f-7df5b91b0a9e',
      gameDayId: '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
      checkedInAtUtc: '2026-05-05T12:00:00.000Z',
      latitude: -23.5505,
      longitude: -46.6333,
    })

    expect(parsed.playerId).toBeTypeOf('string')
    expect(parsed.latitude).toBeCloseTo(-23.5505)
  })

  it('deve falhar com playerId inválido', () => {
    expect(() =>
      checkinFormSchema.parse({
        playerId: '   ',
        gameDayId: '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
        latitude: -23.5505,
        longitude: -46.6333,
      }),
    ).toThrow('Jogador inválido')
  })

  it('deve falhar com latitude inválida', () => {
    expect(() =>
      checkinFormSchema.parse({
        playerId: 'player-1',
        gameDayId: '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
        latitude: -91,
        longitude: -46.6333,
      }),
    ).toThrow('Latitude inválida')
  })

  it('deve falhar com longitude inválida', () => {
    expect(() =>
      checkinFormSchema.parse({
        playerId: 'player-1',
        gameDayId: '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
        latitude: -23.5505,
        longitude: 181,
      }),
    ).toThrow('Longitude inválida')
  })
})
