/** Espelha LoginRequest do backend (AuthController). */
export interface LoginRequest {
  email: string
  password: string
}

/** Espelha AuthResponse do backend (AuthResponse record). */
export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
  primaryTenant?: TenantMembership | null
  tenants?: TenantMembership[]
}

export interface TenantMembership {
  id: string
  name: string
  slug: string
  isOwner: boolean
  joinedAt: string
}

/** Perfil do usuário autenticado retornado por GET /api/v1/auth/me. */
export interface UserProfile {
  id: string
  email: string
  roles: string[]
  isActive: boolean
  createdAt: string
  primaryTenant?: TenantMembership | null
  tenants?: TenantMembership[]
}

/** Payload enviado para POST /api/v1/auth/logout. */
export interface LogoutRequest {
  refreshToken: string
}

export interface TenantContext {
  slug: string
  source: 'subdomain' | 'query' | 'profile'
}

/** Espelha TenantResponse do backend (TenantController). */
export interface TenantResponse {
  id: string
  name: string
  slug: string
  provisioningStatus: string
  logoPath?: string | null
  street?: string | null
  number?: string | null
  neighborhood?: string | null
  city?: string | null
  state?: string | null
  zipCode?: string | null
}

/** Payload enviado para POST /api/v1/tenant. */
export interface CreateTenantRequest {
  name: string
  slug: string
  adminEmail: string
  adminPassword: string
}
