import { beforeEach, describe, expect, it } from 'vitest'
import { useTeamStore } from '../teamStore'

describe('teamStore', () => {
  beforeEach(() => {
    useTeamStore.setState({
      search: '',
      selectedTeamId: null,
      modalMode: 'create',
      isTeamModalOpen: false,
      isRosterModalOpen: false,
    })
  })

  it('deve atualizar busca', () => {
    useTeamStore.getState().setSearch('azul')

    expect(useTeamStore.getState().search).toBe('azul')
  })

  it('deve abrir modal de criação', () => {
    useTeamStore.getState().openCreateModal()
    const state = useTeamStore.getState()

    expect(state.modalMode).toBe('create')
    expect(state.selectedTeamId).toBeNull()
    expect(state.isTeamModalOpen).toBe(true)
  })

  it('deve abrir modal de edição', () => {
    useTeamStore.getState().openEditModal('team-1')
    const state = useTeamStore.getState()

    expect(state.modalMode).toBe('edit')
    expect(state.selectedTeamId).toBe('team-1')
    expect(state.isTeamModalOpen).toBe(true)
  })

  it('deve abrir modal de elenco', () => {
    useTeamStore.getState().openRosterModal('team-1')
    const state = useTeamStore.getState()

    expect(state.selectedTeamId).toBe('team-1')
    expect(state.isRosterModalOpen).toBe(true)
  })
})
