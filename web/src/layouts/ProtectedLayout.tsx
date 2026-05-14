import { Outlet } from '@tanstack/react-router'
import { AuthHeader } from '@/core/components/AuthHeader'
import { useCurrentUser } from '@/features/auth/hooks/useCurrentUser'

/** Layout para rotas protegidas (autenticadas). */
export function ProtectedLayout() {
  // Carrega o perfil do usuário assim que o layout monta
  useCurrentUser()

  return (
    <div className="min-h-screen bg-gray-100">
      <AuthHeader />
      <Outlet />
    </div>
  )
}
