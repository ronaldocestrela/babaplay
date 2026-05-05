import { useAuthStore } from '@/features/auth/store/authStore'
import { useLogout } from '@/features/auth/hooks/useLogout'
import { useNavigate } from '@tanstack/react-router'
import { isTenantAdmin } from '@/features/auth/utils/tenantAccess'

export function AuthHeader() {
  const currentUser = useAuthStore((s) => s.currentUser)
  const currentTenant = useAuthStore((s) => s.currentTenant)
  const { logout, isPending } = useLogout()
  const navigate = useNavigate()
  const canEditTenantSettings = isTenantAdmin(currentUser, currentTenant)

  return (
    <header className="bg-white border-b border-gray-200 px-4 py-3">
      <div className="max-w-7xl mx-auto flex items-center justify-between gap-4">
        <div className="flex items-center gap-6">
          <span className="text-lg font-bold text-indigo-600">BabaPlay</span>

          <nav className="hidden md:flex items-center gap-2" aria-label="Navegação principal">
            <button
              type="button"
              onClick={() => navigate({ to: '/' })}
              className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
              disabled={isPending}
            >
              Dashboard
            </button>
            <button
              type="button"
              onClick={() => navigate({ to: '/players' })}
              className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
              disabled={isPending}
            >
              Jogadores
            </button>
            <button
              type="button"
              onClick={() => navigate({ to: '/checkins' })}
              className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
              disabled={isPending}
            >
              Check-ins
            </button>
            <button
              type="button"
              onClick={() => navigate({ to: '/teams' })}
              className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
              disabled={isPending}
            >
              Times
            </button>
            <button
              type="button"
              onClick={() => navigate({ to: '/matches' })}
              className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
              disabled={isPending}
            >
              Partidas
            </button>
            {canEditTenantSettings && (
              <button
                type="button"
                onClick={() => navigate({ to: '/tenant/settings' })}
                className="px-3 py-1.5 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
                disabled={isPending}
              >
                Opções do Tenant
              </button>
            )}
          </nav>
        </div>

        <div className="flex items-center gap-3">
          {currentUser && (
            <span className="text-sm text-gray-600 hidden sm:inline">
              {currentUser.email}
            </span>
          )}
          <button
            type="button"
            onClick={() => logout()}
            disabled={isPending}
            className="px-3 py-1.5 rounded-lg bg-red-50 text-red-700 text-sm font-medium hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-400 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isPending ? 'Saindo...' : 'Sair'}
          </button>
        </div>
      </div>
    </header>
  )
}
