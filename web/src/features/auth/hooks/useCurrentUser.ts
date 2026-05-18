import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'

export const CURRENT_USER_QUERY_KEY = ['auth', 'me'] as const

export function useCurrentUser() {
  const { isAuthenticated, setCurrentUser, setCurrentTenant } = useAuthStore()

  return useQuery({
    queryKey: CURRENT_USER_QUERY_KEY,
    queryFn: async () => {
      const user = await authService.getCurrentUser()
      setCurrentUser(user)

      const currentTenant = useAuthStore.getState().currentTenant
      const memberships = user.tenants ?? []
      if (memberships.length === 0) {
        setCurrentTenant(null)
        return user
      }

      if (memberships.length === 1) {
        setCurrentTenant({ slug: memberships[0].slug, source: 'profile' })
        return user
      }

      // Multi-tenant users must choose explicitly after login.
      const stillMember =
        currentTenant !== null && memberships.some((tenant) => tenant.slug === currentTenant.slug)

      if (!stillMember) {
        setCurrentTenant(null)
      }

      return user
    },
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000, // 5 minutos
    retry: false,
  })
}
