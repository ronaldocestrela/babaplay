import { describe, expect, it } from 'vitest'
import { playerService } from '../playerService'

describe('playerService', () => {
  it('deve listar jogadores', async () => {
    const players = await playerService.getPlayers()

    expect(players.length).toBeGreaterThan(0)
    expect(players[0]?.name).toBeTypeOf('string')
  })

  it('deve buscar jogador por id', async () => {
    const player = await playerService.getPlayerById('player-1')

    expect(player.id).toBe('player-1')
    expect(player.userId).toBeTypeOf('string')
  })

  it('deve criar jogador', async () => {
    const created = await playerService.createPlayer({
      userId: 'user-999',
      name: 'Novo Jogador',
      nickname: 'N10',
      phone: '11999999999',
      dateOfBirth: '1999-01-10',
    })

    expect(created.id).toBeDefined()
    expect(created.name).toBe('Novo Jogador')
  })

  it('deve atualizar jogador', async () => {
    const updated = await playerService.updatePlayer('player-1', {
      name: 'Jogador Atualizado',
      nickname: 'ATU',
      phone: '11888888888',
      dateOfBirth: '1998-08-20',
    })

    expect(updated.id).toBe('player-1')
    expect(updated.name).toBe('Jogador Atualizado')
  })

  it('deve excluir jogador', async () => {
    await expect(playerService.deletePlayer('player-2')).resolves.toBeUndefined()
  })

  it('deve listar posições', async () => {
    const positions = await playerService.getPositions()

    expect(positions.length).toBeGreaterThan(0)
    expect(positions[0]?.code).toBeTypeOf('string')
  })

  it('deve atualizar posições do jogador', async () => {
    const response = await playerService.updatePlayerPositions('player-1', {
      positionIds: ['position-1', 'position-2'],
    })

    expect(response.playerId).toBe('player-1')
    expect(response.positionIds).toHaveLength(2)
  })
})