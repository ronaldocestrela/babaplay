import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthResponse, UserProfile } from '../types'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  currentUser: UserProfile | null
  setTokens: (auth: AuthResponse) => void
  setCurrentUser: (user: UserProfile) => void
  clearTokens: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      currentUser: null,

      setTokens: (auth: AuthResponse) =>
        set({
          accessToken: auth.accessToken,
          refreshToken: auth.refreshToken,
          isAuthenticated: true,
        }),

      setCurrentUser: (user: UserProfile) =>
        set({ currentUser: user }),

      clearTokens: () =>
        set({
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
          currentUser: null,
        }),
    }),
    { name: 'auth-storage' },
  ),
)
