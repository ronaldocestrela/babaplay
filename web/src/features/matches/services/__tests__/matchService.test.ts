import { describe, expect, it } from 'vitest'
import { matchService } from '../matchService'

describe('matchService', () => {
  it('deve listar partidas', async () => {
    const matches = await matchService.getMatches()

    expect(matches.length).toBeGreaterThan(0)
    expect(matches[0]?.status).toBeTypeOf('string')
  })

  it('deve buscar partida por id', async () => {
    const match = await matchService.getMatchById('match-1')

    expect(match.id).toBe('match-1')
    expect(match.homeTeamId).toBeDefined()
  })

  it('deve criar partida', async () => {
    const created = await matchService.createMatch({
      gameDayId: 'gameday-2',
      homeTeamId: 'team-1',
      awayTeamId: 'team-3',
      description: 'Partida teste',
    })

    expect(created.id).toBeDefined()
    expect(created.status).toBe('Pending')
  })

  it('deve atualizar partida', async () => {
    const updated = await matchService.updateMatch('match-1', {
      gameDayId: 'gameday-1',
      homeTeamId: 'team-1',
      awayTeamId: 'team-2',
      description: 'Partida atualizada',
    })

    expect(updated.id).toBe('match-1')
    expect(updated.description).toBe('Partida atualizada')
  })

  it('deve alterar status da partida', async () => {
    const updated = await matchService.changeStatus('match-1', {
      status: 'Completed',
    })

    expect(updated.status).toBe('Completed')
  })

  it('deve excluir partida', async () => {
    await expect(matchService.deleteMatch('match-2')).resolves.toBeUndefined()
  })

  it('deve listar dados auxiliares para formulário', async () => {
    const [gameDays, teams] = await Promise.all([
      matchService.getGameDaysForMatches(),
      matchService.getTeamsForMatches(),
    ])

    expect(gameDays.length).toBeGreaterThan(0)
    expect(teams.length).toBeGreaterThan(0)
  })
})
