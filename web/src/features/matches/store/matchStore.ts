import { create } from 'zustand'

type MatchModalMode = 'create' | 'edit'

interface MatchUiState {
  search: string
  selectedMatchId: string | null
  modalMode: MatchModalMode
  isModalOpen: boolean
  setSearch: (value: string) => void
  openCreateModal: () => void
  openEditModal: (matchId: string) => void
  closeModal: () => void
}

export const useMatchStore = create<MatchUiState>((set) => ({
  search: '',
  selectedMatchId: null,
  modalMode: 'create',
  isModalOpen: false,
  setSearch: (value) => set({ search: value }),
  openCreateModal: () =>
    set({
      modalMode: 'create',
      selectedMatchId: null,
      isModalOpen: true,
    }),
  openEditModal: (matchId) =>
    set({
      modalMode: 'edit',
      selectedMatchId: matchId,
      isModalOpen: true,
    }),
  closeModal: () => set({ isModalOpen: false }),
}))
