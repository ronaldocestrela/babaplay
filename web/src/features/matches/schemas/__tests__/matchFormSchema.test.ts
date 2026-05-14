import { describe, expect, it } from 'vitest'
import { matchFormSchema } from '../matchFormSchema'

describe('matchFormSchema', () => {
  it('deve validar payload válido', () => {
    const parsed = matchFormSchema.parse({
      gameDayId: 'gameday-1',
      homeTeamId: 'team-1',
      awayTeamId: 'team-2',
      description: 'Semifinal',
    })

    expect(parsed.gameDayId).toBe('gameday-1')
  })

  it('deve impedir times iguais', () => {
    expect(() =>
      matchFormSchema.parse({
        gameDayId: 'gameday-1',
        homeTeamId: 'team-1',
        awayTeamId: 'team-1',
      }),
    ).toThrow('Times mandante e visitante devem ser diferentes')
  })

  it('deve exigir dia de jogo', () => {
    expect(() =>
      matchFormSchema.parse({
        gameDayId: '',
        homeTeamId: 'team-1',
        awayTeamId: 'team-2',
      }),
    ).toThrow('Dia de jogo é obrigatório')
  })

  it('deve limitar tamanho da descrição', () => {
    expect(() =>
      matchFormSchema.parse({
        gameDayId: 'gameday-1',
        homeTeamId: 'team-1',
        awayTeamId: 'team-2',
        description: 'a'.repeat(501),
      }),
    ).toThrow('Descrição deve ter no máximo 500 caracteres')
  })
})
