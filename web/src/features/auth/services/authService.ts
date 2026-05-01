import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { LoginRequest, AuthResponse } from '../types'

export const authService = {
  login: (data: LoginRequest): Promise<AuthResponse> =>
    apiClient
      .post<AuthResponse>(API_ROUTES.AUTH.LOGIN, data)
      .then((res) => res.data),

  refreshToken: (refreshToken: string): Promise<AuthResponse> =>
    apiClient
      .post<AuthResponse>(API_ROUTES.AUTH.REFRESH_TOKEN, { refreshToken })
      .then((res) => res.data),
}
