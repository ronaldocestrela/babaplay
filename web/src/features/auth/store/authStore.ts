import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthResponse, TenantContext, UserProfile } from '../types'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  currentUser: UserProfile | null
  currentTenant: TenantContext | null
  setTokens: (auth: AuthResponse) => void
  setCurrentUser: (user: UserProfile) => void
  setCurrentTenant: (tenant: TenantContext | null) => void
  clearTokens: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      currentUser: null,
      currentTenant: null,

      setTokens: (auth: AuthResponse) =>
        set({
          accessToken: auth.accessToken,
          refreshToken: auth.refreshToken,
          isAuthenticated: true,
        }),

      setCurrentUser: (user: UserProfile) =>
        set({ currentUser: user }),

      setCurrentTenant: (tenant: TenantContext | null) =>
        set({ currentTenant: tenant }),

      clearTokens: () =>
        set({
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
          currentUser: null,
          currentTenant: null,
        }),
    }),
    { name: 'auth-storage' },
  ),
)
