import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { useAuthStore } from '../store/authStore'
import { authService } from '../services/authService'

export function useLogout() {
  const { refreshToken, clearTokens } = useAuthStore()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { mutate: logout, isPending } = useMutation({
    mutationFn: () => authService.logout(refreshToken ?? ''),
    onSettled: () => {
      // Limpa sessão e cache independente de sucesso ou falha do servidor
      clearTokens()
      queryClient.clear()
      void navigate({ to: '/login' })
    },
  })

  return { logout, isPending }
}
