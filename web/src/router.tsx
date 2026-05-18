import {
  createRouter,
  createRootRoute,
  createRoute,
  Outlet,
  redirect,
} from '@tanstack/react-router'
import { useAuthStore } from '@/features/auth/store/authStore'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { PlayersPage } from '@/pages/PlayersPage'
import { CheckinsPage } from '@/pages/CheckinsPage'
import { TeamsPage } from '@/pages/TeamsPage'
import { MatchesPage } from '@/pages/MatchesPage'
import { PositionsPage } from '@/pages/PositionsPage'
import { TenantSettingsPage } from '@/pages/TenantSettingsPage'
import { PublicLayout } from '@/layouts/PublicLayout'
import { ProtectedLayout } from '@/layouts/ProtectedLayout'
import { RegisterAssociationPage } from '@/pages/RegisterAssociationPage'
import { AcceptAssociationInvitePage } from '@/pages/AcceptAssociationInvitePage'
import { CompletePlayerProfilePage } from '@/pages/CompletePlayerProfilePage'
import { SelectTenantPage } from '@/pages/SelectTenantPage'

// ── Root ─────────────────────────────────────────────────────────────────────
const rootRoute = createRootRoute({
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

const registerAssociationRoute = createRoute({
  getParentRoute: () => publicRoute,
  path: '/register-association',
  component: RegisterAssociationPage,
})

const acceptAssociationInviteRoute = createRoute({
  getParentRoute: () => publicRoute,
  path: '/invite/accept',
  component: AcceptAssociationInvitePage,
})

// ── Protected routes ──────────────────────────────────────────────────────────
const protectedRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: 'protected',
  beforeLoad: ({ location }) => {
    const store = useAuthStore.getState()

    if (!store.isAuthenticated) {
      throw redirect({ to: '/login' })
    }

    if (store.requiresPlayerOnboarding && location.pathname !== '/players/complete-profile') {
      throw redirect({ to: '/players/complete-profile' })
    }

    if (location.pathname === '/select-tenant') {
      return
    }

    const memberships = store.currentUser?.tenants ?? []
    if (memberships.length > 1 && store.currentTenant === null) {
      throw redirect({ to: '/select-tenant' })
    }
  },
  component: ProtectedLayout,
})

const selectTenantRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/select-tenant',
  component: SelectTenantPage,
})

const completePlayerProfileRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/players/complete-profile',
  beforeLoad: () => {
    if (!useAuthStore.getState().requiresPlayerOnboarding) {
      throw redirect({ to: '/' })
    }
  },
  component: CompletePlayerProfilePage,
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

const matchesRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/matches',
  component: MatchesPage,
})

const positionsRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/positions',
  component: PositionsPage,
})

const tenantSettingsRoute = createRoute({
  getParentRoute: () => protectedRoute,
  path: '/tenant/settings',
  component: TenantSettingsPage,
})

// ── Router ────────────────────────────────────────────────────────────────────
const routeTree = rootRoute.addChildren([
  publicRoute.addChildren([
    loginRoute,
    registerAssociationRoute,
    acceptAssociationInviteRoute,
  ]),
  protectedRoute.addChildren([
    selectTenantRoute,
    dashboardRoute,
    completePlayerProfileRoute,
    playersRoute,
    checkinsRoute,
    teamsRoute,
    matchesRoute,
    positionsRoute,
    tenantSettingsRoute,
  ]),
])

export const router = createRouter({ routeTree })

// Declaração de tipo global para type-safe navigation
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}
