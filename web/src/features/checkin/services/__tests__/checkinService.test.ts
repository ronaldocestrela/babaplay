import { describe, expect, it } from 'vitest'
import { checkinService } from '../checkinService'

describe('checkinService', () => {
  it('deve listar check-ins por game day', async () => {
    const checkins = await checkinService.getCheckinsByGameDay('gameday-1')

    expect(checkins.length).toBeGreaterThan(0)
    expect(checkins[0]?.gameDayId).toBe('gameday-1')
  })

  it('deve listar check-ins por jogador', async () => {
    const checkins = await checkinService.getCheckinsByPlayer('player-1')

    expect(checkins.length).toBeGreaterThan(0)
    expect(checkins[0]?.playerId).toBe('player-1')
  })

  it('deve criar check-in', async () => {
    const created = await checkinService.createCheckin({
      playerId: 'player-2',
      gameDayId: 'gameday-1',
      checkedInAtUtc: '2026-05-05T10:00:00.000Z',
      latitude: -23.551,
      longitude: -46.633,
    })

    expect(created.id).toBeDefined()
    expect(created.playerId).toBe('player-2')
  })

  it('deve cancelar check-in', async () => {
    await expect(checkinService.cancelCheckin('checkin-1')).resolves.toBeUndefined()
  })
})
