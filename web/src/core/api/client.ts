import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import type { ProblemDetails } from '@/core/types/api'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import { useAuthStore } from '@/features/auth/store/authStore'
import { getTenantFromUrl, TENANT_HEADER_NAME } from '@/features/auth/services/tenantService'

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5050'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

function resolveTenantContext() {
  const state = useAuthStore.getState()
  const urlTenant = getTenantFromUrl()
  const memberships = state.currentUser?.tenants ?? []

  if (urlTenant) {
    if (memberships.length === 0 || memberships.some((tenant) => tenant.slug === urlTenant.slug)) {
      return urlTenant
    }
  }

  return state.currentTenant
}

// ── Request: injeta Bearer token se disponível ──────────────────────────────
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`

  const tenant = resolveTenantContext()
  if (tenant) {
    config.headers[TENANT_HEADER_NAME] = tenant.slug
    useAuthStore.getState().setCurrentTenant(tenant)
  }

  return config
})

// ── Response: renovação silenciosa do access token em caso de 401 ────────────
let refreshing = false
const pendingQueue: Array<{ ok: (t: string) => void; fail: (e: unknown) => void }> = []

function flushQueue(err: unknown, token: string | null) {
  pendingQueue.splice(0).forEach((p) => (err ? p.fail(err) : p.ok(token!)))
}

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError<ProblemDetails>) => {
    const req = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    if (error.response?.status !== 401 || req?._retry) {
      return Promise.reject(error)
    }

    if (refreshing) {
      return new Promise<string>((ok, fail) => pendingQueue.push({ ok, fail })).then(
        (token) => {
          req.headers.Authorization = `Bearer ${token}`
          return apiClient(req)
        },
      )
    }

    req._retry = true
    refreshing = true

    const { refreshToken, clearTokens, setTokens } = useAuthStore.getState()

    if (!refreshToken) {
      refreshing = false
      clearTokens()
      window.location.replace('/login')
      return Promise.reject(error)
    }

    try {
      const tenant = resolveTenantContext()
      const { data } = await axios.post<{
        accessToken: string
        refreshToken: string
        expiresIn: number
        tokenType: string
      }>(
        `${BASE_URL}${API_ROUTES.AUTH.REFRESH_TOKEN}`,
        { refreshToken },
        {
          headers: tenant ? { [TENANT_HEADER_NAME]: tenant.slug } : undefined,
        },
      )

      setTokens(data)
      flushQueue(null, data.accessToken)
      req.headers.Authorization = `Bearer ${data.accessToken}`
      return apiClient(req)
    } catch (refreshError) {
      flushQueue(refreshError, null)
      useAuthStore.getState().clearTokens()
      window.location.replace('/login')
      return Promise.reject(refreshError)
    } finally {
      refreshing = false
    }
  },
)

