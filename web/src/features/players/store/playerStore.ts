import { create } from 'zustand'

type PlayerModalMode = 'create' | 'edit'

interface PlayerUiState {
  search: string
  selectedPlayerId: string | null
  modalMode: PlayerModalMode
  isModalOpen: boolean
  setSearch: (value: string) => void
  openCreateModal: () => void
  openEditModal: (playerId: string) => void
  closeModal: () => void
}

export const usePlayerStore = create<PlayerUiState>((set) => ({
  search: '',
  selectedPlayerId: null,
  modalMode: 'create',
  isModalOpen: false,
  setSearch: (value) => set({ search: value }),
  openCreateModal: () =>
    set({
      modalMode: 'create',
      selectedPlayerId: null,
      isModalOpen: true,
    }),
  openEditModal: (playerId) =>
    set({
      modalMode: 'edit',
      selectedPlayerId: playerId,
      isModalOpen: true,
    }),
  closeModal: () => set({ isModalOpen: false }),
}))