import { beforeEach, describe, expect, it } from 'vitest'
import { useMatchStore } from '../matchStore'

describe('matchStore', () => {
  beforeEach(() => {
    useMatchStore.setState({
      search: '',
      selectedMatchId: null,
      modalMode: 'create',
      isModalOpen: false,
    })
  })

  it('deve atualizar busca', () => {
    useMatchStore.getState().setSearch('azul')

    expect(useMatchStore.getState().search).toBe('azul')
  })

  it('deve abrir modal de criação', () => {
    useMatchStore.getState().openCreateModal()
    const state = useMatchStore.getState()

    expect(state.modalMode).toBe('create')
    expect(state.selectedMatchId).toBeNull()
    expect(state.isModalOpen).toBe(true)
  })

  it('deve abrir modal de edição', () => {
    useMatchStore.getState().openEditModal('match-1')
    const state = useMatchStore.getState()

    expect(state.modalMode).toBe('edit')
    expect(state.selectedMatchId).toBe('match-1')
    expect(state.isModalOpen).toBe(true)
  })

  it('deve fechar modal', () => {
    useMatchStore.getState().openCreateModal()
    useMatchStore.getState().closeModal()

    expect(useMatchStore.getState().isModalOpen).toBe(false)
  })
})
