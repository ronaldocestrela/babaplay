import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'
import { getTenantFromUrl } from '../services/tenantService'

export const CURRENT_USER_QUERY_KEY = ['auth', 'me'] as const

export function useCurrentUser() {
  const { isAuthenticated, setCurrentUser, setCurrentTenant } = useAuthStore()

  return useQuery({
    queryKey: CURRENT_USER_QUERY_KEY,
    queryFn: async () => {
      const user = await authService.getCurrentUser()
      setCurrentUser(user)

      const memberships = user.tenants ?? []
      if (memberships.length > 0) {
        const urlTenant = getTenantFromUrl()
        const urlTenantIsMember =
          urlTenant !== null && memberships.some((tenant) => tenant.slug === urlTenant.slug)

        if (urlTenantIsMember) {
          setCurrentTenant(urlTenant)
        } else if (user.primaryTenant?.slug) {
          setCurrentTenant({ slug: user.primaryTenant.slug, source: 'profile' })
        } else {
          setCurrentTenant({ slug: memberships[0].slug, source: 'profile' })
        }
      }

      return user
    },
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000, // 5 minutos
    retry: false,
  })
}
