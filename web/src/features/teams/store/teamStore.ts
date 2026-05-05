import { create } from 'zustand'

type TeamModalMode = 'create' | 'edit'

interface TeamUiState {
  search: string
  selectedTeamId: string | null
  modalMode: TeamModalMode
  isTeamModalOpen: boolean
  isRosterModalOpen: boolean
  setSearch: (value: string) => void
  openCreateModal: () => void
  openEditModal: (teamId: string) => void
  closeTeamModal: () => void
  openRosterModal: (teamId: string) => void
  closeRosterModal: () => void
}

export const useTeamStore = create<TeamUiState>((set) => ({
  search: '',
  selectedTeamId: null,
  modalMode: 'create',
  isTeamModalOpen: false,
  isRosterModalOpen: false,
  setSearch: (value) => set({ search: value }),
  openCreateModal: () =>
    set({
      modalMode: 'create',
      selectedTeamId: null,
      isTeamModalOpen: true,
    }),
  openEditModal: (teamId) =>
    set({
      modalMode: 'edit',
      selectedTeamId: teamId,
      isTeamModalOpen: true,
    }),
  closeTeamModal: () => set({ isTeamModalOpen: false }),
  openRosterModal: (teamId) =>
    set({
      selectedTeamId: teamId,
      isRosterModalOpen: true,
    }),
  closeRosterModal: () => set({ isRosterModalOpen: false }),
}))
