import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { CreatePositionRequest, Position, UpdatePositionRequest } from '../types'

export const positionService = {
  getPositions: (): Promise<Position[]> =>
    apiClient.get<Position[]>(API_ROUTES.POSITION.LIST).then((res) => res.data),

  createPosition: (payload: CreatePositionRequest): Promise<Position> =>
    apiClient.post<Position>(API_ROUTES.POSITION.LIST, payload).then((res) => res.data),

  updatePosition: (id: string, payload: UpdatePositionRequest): Promise<Position> =>
    apiClient.put<Position>(API_ROUTES.POSITION.BY_ID(id), payload).then((res) => res.data),

  deletePosition: async (id: string): Promise<void> => {
    await apiClient.delete(API_ROUTES.POSITION.BY_ID(id))
  },
}