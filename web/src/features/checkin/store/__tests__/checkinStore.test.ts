import { beforeEach, describe, expect, it } from 'vitest'
import { useCheckinStore } from '../checkinStore'

describe('checkinStore', () => {
  beforeEach(() => {
    useCheckinStore.getState().reset()
  })

  it('deve selecionar game day', () => {
    useCheckinStore.getState().setSelectedGameDayId('gameday-1')

    expect(useCheckinStore.getState().selectedGameDayId).toBe('gameday-1')
  })

  it('deve selecionar jogador', () => {
    useCheckinStore.getState().setSelectedPlayerId('player-1')

    expect(useCheckinStore.getState().selectedPlayerId).toBe('player-1')
  })

  it('deve atualizar filtro', () => {
    useCheckinStore.getState().setFilter('all')

    expect(useCheckinStore.getState().filter).toBe('all')
  })

  it('deve abrir mapa', () => {
    useCheckinStore.getState().setIsMapOpen(true)

    expect(useCheckinStore.getState().isMapOpen).toBe(true)
  })
})
