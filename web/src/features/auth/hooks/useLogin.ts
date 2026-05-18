import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { CURRENT_USER_QUERY_KEY } from './useCurrentUser'
import type { LoginRequest } from '../types'
import { getPendingInviteToken, clearPendingInviteToken } from '@/features/tenant-invitations/utils/pendingInviteStorage'
import { invitationService } from '@/features/tenant-invitations/services/invitationService'

export function useLogin() {
  const setTokens = useAuthStore((s) => s.setTokens)
  const setCurrentTenant = useAuthStore((s) => s.setCurrentTenant)
  const setPlayerOnboardingRequired = useAuthStore((s) => s.setPlayerOnboardingRequired)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { mutate: login, isPending, error, isError } = useMutation({
    mutationFn: (data: LoginRequest) => authService.login(data),
    onSuccess: async (auth) => {
      setTokens(auth)
      setCurrentTenant(null)

      await queryClient.prefetchQuery({
        queryKey: CURRENT_USER_QUERY_KEY,
        queryFn: async () => {
          const user = await authService.getCurrentUser()
          const store = useAuthStore.getState()
          store.setCurrentUser(user)

          const memberships = user.tenants ?? []
          if (memberships.length === 1) {
            store.setCurrentTenant({ slug: memberships[0].slug, source: 'profile' })
          } else if (memberships.length > 1) {
            store.setCurrentTenant(null)
          }

          return user
        },
      })

      const pendingInviteToken = getPendingInviteToken()
      if (pendingInviteToken) {
        try {
          const accepted = await invitationService.accept(pendingInviteToken)
          setCurrentTenant({ slug: accepted.tenantSlug, source: 'profile' })
          setPlayerOnboardingRequired(accepted.requiresPlayerProfile)

          await queryClient.invalidateQueries({ queryKey: CURRENT_USER_QUERY_KEY })

          if (accepted.requiresPlayerProfile) {
            void navigate({ to: '/players/complete-profile' })
            return
          }
        } finally {
          clearPendingInviteToken()
        }
      }

      if (useAuthStore.getState().requiresPlayerOnboarding) {
        void navigate({ to: '/players/complete-profile' })
        return
      }

      const tenantCount = useAuthStore.getState().currentUser?.tenants?.length ?? 0
      if (tenantCount > 1 && useAuthStore.getState().currentTenant === null) {
        void navigate({ to: '/select-tenant' })
        return
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
