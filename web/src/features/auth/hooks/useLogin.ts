import { useMutation } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import type { LoginRequest } from '../types'

export function useLogin() {
  const setTokens = useAuthStore((s) => s.setTokens)
  const navigate = useNavigate()

  const { mutate: login, isPending, error, isError } = useMutation({
    mutationFn: (data: LoginRequest) => authService.login(data),
    onSuccess: (auth) => {
      setTokens(auth)
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
