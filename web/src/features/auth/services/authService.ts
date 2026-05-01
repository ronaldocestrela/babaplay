import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { LoginRequest, AuthResponse, UserProfile } from '../types'

export const authService = {
  login: (data: LoginRequest): Promise<AuthResponse> =>
    apiClient
      .post<AuthResponse>(API_ROUTES.AUTH.LOGIN, data)
      .then((res) => res.data),

  refreshToken: (refreshToken: string): Promise<AuthResponse> =>
    apiClient
      .post<AuthResponse>(API_ROUTES.AUTH.REFRESH_TOKEN, { refreshToken })
      .then((res) => res.data),

  logout: (refreshToken: string): Promise<void> =>
    apiClient
      .post(API_ROUTES.AUTH.LOGOUT, { refreshToken })
      .then(() => undefined),

  getCurrentUser: (): Promise<UserProfile> =>
    apiClient
      .get<UserProfile>(API_ROUTES.AUTH.ME)
      .then((res) => res.data),
}
