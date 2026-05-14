import { create } from 'zustand'

type PositionModalMode = 'create' | 'edit'

interface PositionUiState {
  search: string
  selectedPositionId: string | null
  modalMode: PositionModalMode
  isPositionModalOpen: boolean
  setSearch: (value: string) => void
  openCreateModal: () => void
  openEditModal: (positionId: string) => void
  closePositionModal: () => void
}

export const usePositionStore = create<PositionUiState>((set) => ({
  search: '',
  selectedPositionId: null,
  modalMode: 'create',
  isPositionModalOpen: false,
  setSearch: (value) => set({ search: value }),
  openCreateModal: () =>
    set({
      modalMode: 'create',
      selectedPositionId: null,
      isPositionModalOpen: true,
    }),
  openEditModal: (positionId) =>
    set({
      modalMode: 'edit',
      selectedPositionId: positionId,
      isPositionModalOpen: true,
    }),
  closePositionModal: () => set({ isPositionModalOpen: false }),
}))