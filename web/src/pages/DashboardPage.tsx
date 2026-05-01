import { useAuthStore } from '@/features/auth/store/authStore'

export function DashboardPage() {
  const clearTokens = useAuthStore((s) => s.clearTokens)

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <button
          type="button"
          onClick={clearTokens}
          className="px-4 py-2 rounded-lg bg-red-100 text-red-700 text-sm font-medium hover:bg-red-200 transition-colors"
        >
          Sair
        </button>
      </div>
      <p className="text-gray-600">Bem-vindo ao BabaPlay! 🎉</p>
    </div>
  )
}
