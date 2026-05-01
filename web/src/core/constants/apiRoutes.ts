/** Rotas da API — espelha os endpoints definidos no AuthController e futuros controllers. */
export const API_ROUTES = {
  AUTH: {
    LOGIN: '/api/v1/auth/login',
    REFRESH_TOKEN: '/api/v1/auth/refresh-token',
  },
} as const
