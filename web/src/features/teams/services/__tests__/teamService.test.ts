import { describe, expect, it } from 'vitest'
import { teamService } from '../teamService'

describe('teamService', () => {
  it('deve listar times', async () => {
    const teams = await teamService.getTeams()

    expect(teams.length).toBeGreaterThan(0)
    expect(teams[0]?.name).toBeTypeOf('string')
  })

  it('deve buscar time por id', async () => {
    const team = await teamService.getTeamById('team-1')

    expect(team.id).toBe('team-1')
    expect(team.maxPlayers).toBeGreaterThan(0)
  })

  it('deve criar time', async () => {
    const created = await teamService.createTeam({
      name: 'Time Teste',
      maxPlayers: 10,
    })

    expect(created.id).toBeDefined()
    expect(created.name).toBe('Time Teste')
  })

  it('deve atualizar time', async () => {
    const updated = await teamService.updateTeam('team-1', {
      name: 'Time Atualizado',
      maxPlayers: 9,
    })

    expect(updated.id).toBe('team-1')
    expect(updated.name).toBe('Time Atualizado')
  })

  it('deve excluir time', async () => {
    await expect(teamService.deleteTeam('team-2')).resolves.toBeUndefined()
  })

  it('deve atualizar elenco do time', async () => {
    const response = await teamService.updateTeamPlayers('team-1', {
      playerIds: ['player-1', 'player-2'],
    })

    expect(response.teamId).toBe('team-1')
    expect(response.playerIds).toHaveLength(2)
  })

  it('deve listar jogadores para elenco', async () => {
    const players = await teamService.getPlayersForRoster()

    expect(players.length).toBeGreaterThan(0)
    expect(players[0]?.name).toBeTypeOf('string')
  })
})
