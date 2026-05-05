import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type {
  CreatePlayerRequest,
  Player,
  PlayerPositionsResponse,
  Position,
  UpdatePlayerPositionsRequest,
  UpdatePlayerRequest,
} from '../types'

export const playerService = {
  getPlayers: (): Promise<Player[]> =>
    apiClient.get<Player[]>(API_ROUTES.PLAYER.LIST).then((res) => res.data),

  getPlayerById: (id: string): Promise<Player> =>
    apiClient.get<Player>(API_ROUTES.PLAYER.BY_ID(id)).then((res) => res.data),

  createPlayer: (payload: CreatePlayerRequest): Promise<Player> =>
    apiClient.post<Player>(API_ROUTES.PLAYER.LIST, payload).then((res) => res.data),

  updatePlayer: (id: string, payload: UpdatePlayerRequest): Promise<Player> =>
    apiClient.put<Player>(API_ROUTES.PLAYER.BY_ID(id), payload).then((res) => res.data),

  deletePlayer: async (id: string): Promise<void> => {
    await apiClient.delete(API_ROUTES.PLAYER.BY_ID(id))
  },

  getPositions: (): Promise<Position[]> =>
    apiClient.get<Position[]>(API_ROUTES.POSITION.LIST).then((res) => res.data),

  updatePlayerPositions: (
    id: string,
    payload: UpdatePlayerPositionsRequest,
  ): Promise<PlayerPositionsResponse> =>
    apiClient
      .put<PlayerPositionsResponse>(API_ROUTES.PLAYER.UPDATE_POSITIONS(id), payload)
      .then((res) => res.data),
}