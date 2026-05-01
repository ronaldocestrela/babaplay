import { useAuthStore } from '@/features/auth/store/authStore'
import { useLogout } from '@/features/auth/hooks/useLogout'

export function AuthHeader() {
  const currentUser = useAuthStore((s) => s.currentUser)
  const { logout, isPending } = useLogout()

  return (
    <header className="bg-white border-b border-gray-200 px-4 py-3">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        <span className="text-lg font-bold text-indigo-600">BabaPlay</span>

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
