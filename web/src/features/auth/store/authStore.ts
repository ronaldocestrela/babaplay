import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthResponse, TenantContext, UserProfile } from '../types'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  currentUser: UserProfile | null
  currentTenant: TenantContext | null
  requiresPlayerOnboarding: boolean
  setTokens: (auth: AuthResponse) => void
  setCurrentUser: (user: UserProfile) => void
  setCurrentTenant: (tenant: TenantContext | null) => void
  setPlayerOnboardingRequired: (required: boolean) => void
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
      requiresPlayerOnboarding: false,

      setTokens: (auth: AuthResponse) =>
        set({
          accessToken: auth.accessToken,
          refreshToken: auth.refreshToken,
          isAuthenticated: true,
          requiresPlayerOnboarding: false,
        }),

      setCurrentUser: (user: UserProfile) =>
        set({ currentUser: user }),

      setCurrentTenant: (tenant: TenantContext | null) =>
        set({ currentTenant: tenant }),

      setPlayerOnboardingRequired: (required: boolean) =>
        set({ requiresPlayerOnboarding: required }),

      clearTokens: () =>
        set({
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
          currentUser: null,
          currentTenant: null,
          requiresPlayerOnboarding: false,
        }),
    }),
    { name: 'auth-storage' },
  ),
)
