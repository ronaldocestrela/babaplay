import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type {
  ChangeTenantGameDayOptionStatusRequest,
  CreateTenantGameDayOptionRequest,
  TenantGameDayOptionResponse,
} from '../types/gameDayOptions'

export const tenantGameDayOptionsService = {
  getOptions: (): Promise<TenantGameDayOptionResponse[]> =>
    apiClient
      .get<TenantGameDayOptionResponse[]>(API_ROUTES.TENANT.GAME_DAY_OPTIONS)
      .then((res) => res.data),

  createOption: (payload: CreateTenantGameDayOptionRequest): Promise<TenantGameDayOptionResponse> =>
    apiClient
      .post<TenantGameDayOptionResponse>(API_ROUTES.TENANT.GAME_DAY_OPTIONS, payload)
      .then((res) => res.data),

  changeStatus: ({ id, isActive }: ChangeTenantGameDayOptionStatusRequest): Promise<TenantGameDayOptionResponse> =>
    apiClient
      .put<TenantGameDayOptionResponse>(API_ROUTES.TENANT.GAME_DAY_OPTION_STATUS(id), { isActive })
      .then((res) => res.data),
}
