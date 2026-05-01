import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'

export const CURRENT_USER_QUERY_KEY = ['auth', 'me'] as const

export function useCurrentUser() {
  const { isAuthenticated, setCurrentUser } = useAuthStore()

  return useQuery({
    queryKey: CURRENT_USER_QUERY_KEY,
    queryFn: async () => {
      const user = await authService.getCurrentUser()
      setCurrentUser(user)
      return user
    },
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000, // 5 minutos
    retry: false,
  })
}
