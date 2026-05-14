import { beforeEach, describe, expect, it } from 'vitest'
import { usePlayerStore } from '../playerStore'

describe('playerStore', () => {
  beforeEach(() => {
    usePlayerStore.setState({
      search: '',
      selectedPlayerId: null,
      modalMode: 'create',
      isModalOpen: false,
    })
  })

  it('deve atualizar termo de busca', () => {
    usePlayerStore.getState().setSearch('joao')

    expect(usePlayerStore.getState().search).toBe('joao')
  })

  it('deve abrir modal no modo create', () => {
    usePlayerStore.getState().openCreateModal()

    const state = usePlayerStore.getState()
    expect(state.isModalOpen).toBe(true)
    expect(state.modalMode).toBe('create')
    expect(state.selectedPlayerId).toBeNull()
  })

  it('deve abrir modal no modo edit', () => {
    usePlayerStore.getState().openEditModal('player-1')

    const state = usePlayerStore.getState()
    expect(state.isModalOpen).toBe(true)
    expect(state.modalMode).toBe('edit')
    expect(state.selectedPlayerId).toBe('player-1')
  })

  it('deve fechar modal', () => {
    usePlayerStore.getState().openCreateModal()
    usePlayerStore.getState().closeModal()

    expect(usePlayerStore.getState().isModalOpen).toBe(false)
  })
})