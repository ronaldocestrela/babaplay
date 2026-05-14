import { create } from 'zustand'

type CheckinFilter = 'active' | 'all'

interface CheckinUiState {
  selectedGameDayId: string | null
  selectedPlayerId: string | null
  filter: CheckinFilter
  isMapOpen: boolean
  setSelectedGameDayId: (gameDayId: string | null) => void
  setSelectedPlayerId: (playerId: string | null) => void
  setFilter: (filter: CheckinFilter) => void
  setIsMapOpen: (open: boolean) => void
  reset: () => void
}

const initialState = {
  selectedGameDayId: null,
  selectedPlayerId: null,
  filter: 'active' as CheckinFilter,
  isMapOpen: false,
}

export const useCheckinStore = create<CheckinUiState>((set) => ({
  ...initialState,
  setSelectedGameDayId: (selectedGameDayId) => set({ selectedGameDayId }),
  setSelectedPlayerId: (selectedPlayerId) => set({ selectedPlayerId }),
  setFilter: (filter) => set({ filter }),
  setIsMapOpen: (isMapOpen) => set({ isMapOpen }),
  reset: () => set(initialState),
}))
