import { Outlet } from '@tanstack/react-router'

/** Layout para rotas públicas (ex.: login). */
export function PublicLayout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Outlet />
    </div>
  )
}
