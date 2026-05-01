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
}

/** Perfil do usuário autenticado retornado por GET /api/v1/auth/me. */
export interface UserProfile {
  id: string
  email: string
  roles: string[]
  isActive: boolean
  createdAt: string
}

/** Payload enviado para POST /api/v1/auth/logout. */
export interface LogoutRequest {
  refreshToken: string
}
