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
