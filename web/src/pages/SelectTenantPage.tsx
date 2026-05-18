import { useNavigate } from '@tanstack/react-router'
import { useEffect } from 'react'
import { useAuthStore } from '@/features/auth/store/authStore'

export function SelectTenantPage() {
  const navigate = useNavigate()
  const currentUser = useAuthStore((s) => s.currentUser)
  const setCurrentTenant = useAuthStore((s) => s.setCurrentTenant)

  const memberships = currentUser?.tenants ?? []

  useEffect(() => {
    if (memberships.length !== 1) {
      return
    }

    setCurrentTenant({ slug: memberships[0].slug, source: 'profile' })
    void navigate({ to: '/' })
  }, [memberships, navigate, setCurrentTenant])

  if (memberships.length === 0) {
    return (
      <main className="min-h-screen grid place-items-center p-6">
        <div className="w-full max-w-md rounded-xl border border-amber-200 bg-amber-50 p-6 text-center">
          <h1 className="text-xl font-semibold text-amber-900">Sem associação ativa</h1>
          <p className="mt-2 text-sm text-amber-800">
            Seu usuário não possui vínculo com nenhuma associação ativa.
          </p>
        </div>
      </main>
    )
  }

  if (memberships.length === 1) {
    return null
  }

  return (
    <main className="min-h-screen grid place-items-center p-6">
      <section className="w-full max-w-lg rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-semibold text-gray-900">Selecione a associação</h1>
        <p className="mt-1 text-sm text-gray-600">
          Este usuário participa de mais de uma associação. Escolha com qual deseja continuar.
        </p>

        <ul className="mt-6 space-y-3">
          {memberships.map((tenant) => (
            <li key={tenant.id}>
              <button
                type="button"
                onClick={() => {
                  setCurrentTenant({ slug: tenant.slug, source: 'selection' })
                  void navigate({ to: '/' })
                }}
                className="w-full rounded-lg border border-gray-200 px-4 py-3 text-left hover:border-indigo-300 hover:bg-indigo-50"
              >
                <p className="text-sm font-semibold text-gray-900">{tenant.name}</p>
                <p className="text-xs text-gray-500">{tenant.slug}</p>
              </button>
            </li>
          ))}
        </ul>
      </section>
    </main>
  )
}
