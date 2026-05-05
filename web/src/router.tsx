import {
  createRouter,
  createRootRoute,
  createRoute,
  Outlet,
  redirect,
} from '@tanstack/react-router'
import { useAuthStore } from '@/features/auth/store/authStore'
import { getTenantFromUrl } from '@/features/auth/services/tenantService'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { PlayersPage } from '@/pages/PlayersPage'
import { CheckinsPage } from '@/pages/CheckinsPage'
import { TeamsPage } from '@/pages/TeamsPage'
import { PublicLayout } from '@/layouts/PublicLayout'
import { ProtectedLayout } from '@/layouts/ProtectedLayout'

// ── Root ─────────────────────────────────────────────────────────────────────
const rootRoute = createRootRoute({
  // Resolve tenant once per navigation; store it so components and API
  // interceptors always have access to the current tenant context.
  beforeLoad: () => {
    const tenant = getTenantFromUrl()
    useAuthStore.getState().setCurrentTenant(tenant)
  },
  component: () => <Outlet />,
})

// ── Public routes ─────────────────────────────────────────────────────────────
const publicRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: 'public',
  component: PublicLayout,
})

const loginRoute = createRoute({
  getParentRoute: () => publicRoute,
  path: '/login',
  beforeLoad: () => {
    if (useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/' })
    }
  },
  component: LoginPage,
})

// ── Protected routes ──────────────────────────────────────────────────────────
const protectedRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: 'protected',
  beforeLoad: () => {
    if (!useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/login' })
    }
  },
  component: ProtectedLayout,
})

const dashboardRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/',
  component: DashboardPage,
})

const playersRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/players',
  component: PlayersPage,
})

const checkinsRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/checkins',
  component: CheckinsPage,
})

const teamsRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/teams',
  component: TeamsPage,
})

// ── Router ────────────────────────────────────────────────────────────────────
const routeTree = rootRoute.addChildren([
  publicRoute.addChildren([loginRoute]),
  protectedRoute.addChildren([dashboardRoute, playersRoute, checkinsRoute, teamsRoute]),
])

export const router = createRouter({ routeTree })

// Declaração de tipo global para type-safe navigation
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}
