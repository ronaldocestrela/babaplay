import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { CURRENT_USER_QUERY_KEY } from './useCurrentUser'
import type { LoginRequest } from '../types'

export function useLogin() {
  const setTokens = useAuthStore((s) => s.setTokens)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { mutate: login, isPending, error, isError } = useMutation({
    mutationFn: (data: LoginRequest) => authService.login(data),
    onSuccess: async (auth) => {
      setTokens(auth)
      await queryClient.prefetchQuery({
        queryKey: CURRENT_USER_QUERY_KEY,
        queryFn: async () => {
          const user = await authService.getCurrentUser()
          useAuthStore.getState().setCurrentUser(user)
          return user
        },
      })
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
