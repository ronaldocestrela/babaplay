import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'
import { getTenantFromUrl } from '../services/tenantService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { CURRENT_USER_QUERY_KEY } from './useCurrentUser'
import type { LoginRequest } from '../types'
import { getPendingInviteToken, clearPendingInviteToken } from '@/features/tenant-invitations/utils/pendingInviteStorage'
import { invitationService } from '@/features/tenant-invitations/services/invitationService'

export function useLogin() {
  const setTokens = useAuthStore((s) => s.setTokens)
  const setCurrentTenant = useAuthStore((s) => s.setCurrentTenant)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { mutate: login, isPending, error, isError } = useMutation({
    mutationFn: (data: LoginRequest) => authService.login(data),
    onSuccess: async (auth) => {
      setTokens(auth)

      if (auth.primaryTenant?.slug) {
        setCurrentTenant({ slug: auth.primaryTenant.slug, source: 'profile' })
      }

      await queryClient.prefetchQuery({
        queryKey: CURRENT_USER_QUERY_KEY,
        queryFn: async () => {
          const user = await authService.getCurrentUser()
          const store = useAuthStore.getState()
          store.setCurrentUser(user)

          const urlTenant = getTenantFromUrl()
          const memberships = user.tenants ?? []
          if (memberships.length > 0) {
            const urlTenantIsMember =
              urlTenant !== null && memberships.some((tenant) => tenant.slug === urlTenant.slug)

            if (urlTenantIsMember) {
              store.setCurrentTenant(urlTenant)
            } else if (user.primaryTenant?.slug) {
              store.setCurrentTenant({ slug: user.primaryTenant.slug, source: 'profile' })
            } else {
              store.setCurrentTenant({ slug: memberships[0].slug, source: 'profile' })
            }
          }

          return user
        },
      })

      const pendingInviteToken = getPendingInviteToken()
      if (pendingInviteToken) {
        try {
          const accepted = await invitationService.accept(pendingInviteToken)
          setCurrentTenant({ slug: accepted.tenantSlug, source: 'profile' })
        } finally {
          clearPendingInviteToken()
        }
      }

      void navigate({ to: '/' })
    },
  })

  return {
    login,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
