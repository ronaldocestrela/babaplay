import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { Checkin, CreateCheckinRequest } from '../types'

export const checkinService = {
  getCheckinsByGameDay: (gameDayId: string): Promise<Checkin[]> =>
    apiClient
      .get<Checkin[]>(API_ROUTES.CHECKIN.BY_GAMEDAY(gameDayId))
      .then((res) => res.data),

  getCheckinsByPlayer: (playerId: string): Promise<Checkin[]> =>
    apiClient.get<Checkin[]>(API_ROUTES.CHECKIN.BY_PLAYER(playerId)).then((res) => res.data),

  createCheckin: (payload: CreateCheckinRequest): Promise<Checkin> =>
    apiClient.post<Checkin>(API_ROUTES.CHECKIN.LIST, payload).then((res) => res.data),

  cancelCheckin: async (id: string): Promise<void> => {
    await apiClient.delete(API_ROUTES.CHECKIN.BY_ID(id))
  },
}
