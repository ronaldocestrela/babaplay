import { Outlet } from '@tanstack/react-router'

/** Layout para rotas protegidas (autenticadas). */
export function ProtectedLayout() {
  return (
    <div className="min-h-screen bg-gray-100">
      <Outlet />
    </div>
  )
}
